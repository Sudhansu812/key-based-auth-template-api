using DotNetEnv;
using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Automation.Reports;
using KeyBasedAuthenticationTemplate_Automation.Reports.Definitions;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

// Exit codes: 0 = all reports generated and delivered, 1 = at least one report/email failed,
// 2 = could not start (bad configuration or unexpected crash).
const int ExitSuccess = 0;
const int ExitReportFailures = 1;
const int ExitFatal = 2;

if (!RunModeParser.TryParse(args, out var runMode))
{
    Console.Error.WriteLine(RunModeParser.Usage);
    return ExitFatal;
}

LoadEnvFile();

var builder = Host.CreateApplicationBuilder(args);

// Logging is configured first (from the raw config value) so that configuration errors
// found below still end up in the log file.
var logDirectory = builder.Configuration[$"{AutomationOptions.SectionName}:{nameof(AutomationOptions.LogDirectory)}"] is { Length: > 0 } configured
    ? configured
    : "logs";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        new JsonFormatter(renderMessage: true),
        Path.Combine(logDirectory, "automation-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: true);

try
{
    builder.Services.AddOptions<AutomationOptions>()
        .Bind(builder.Configuration.GetSection(AutomationOptions.SectionName))
        .ValidateDataAnnotations();

    // The API key is attached once, here, as a default header - it is never logged.
    builder.Services.AddHttpClient<IReportApiClient, ReportApiClient>((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<AutomationOptions>>().Value;
        client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(options.ApiTimeoutSeconds);
        client.DefaultRequestHeaders.Add(ApiKeyHeader.Name, options.ApiKey);
    });

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddSingleton<IExcelReportWriter, ClosedXmlReportWriter>();
    builder.Services.AddTransient<IReport, ProductsEfReport>();
    builder.Services.AddTransient<IReport, UsersEfReport>();
    builder.Services.AddTransient<IReport, ProductsDapperReport>();
    builder.Services.AddTransient<IReport, UsersDapperReport>();
    builder.Services.AddTransient<ReportRunner>();

    using var host = builder.Build();

    // Resolve once up front so invalid configuration fails fast with a clear message.
    _ = host.Services.GetRequiredService<IOptions<AutomationOptions>>().Value;

    using var cancellation = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cancellation.Cancel();
    };

    var allSucceeded = await host.Services.GetRequiredService<ReportRunner>().RunAsync(runMode, cancellation.Token);
    return allSucceeded ? ExitSuccess : ExitReportFailures;
}
catch (OptionsValidationException ex)
{
    Log.Fatal("Automation configuration is invalid: {Failures}", ex.Failures);
    return ExitFatal;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Automation terminated unexpectedly");
    return ExitFatal;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// The .env file lives at <solution root>/env/.env (shared with the API). Walk up from the
// working directory and the build output to find it, so it works from `dotnet run` in the
// project folder, from the solution root, or from bin/. Real environment variables win.
static void LoadEnvFile()
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "env", ".env");
            if (File.Exists(candidate))
            {
                Env.NoClobber().Load(candidate);
                return;
            }
        }
    }
}
