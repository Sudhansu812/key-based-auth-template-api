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

// appsettings.json is read from the application folder (copied next to the exe), not the working
// directory, so the job behaves the same however and wherever it is launched.
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Logging is configured first, from the "Serilog" section of appsettings.json (sinks, log file path,
// retention, levels, format), so configuration errors found below still end up in the log file.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: true);

try
{
    builder.Services.AddOptions<AutomationOptions>()
        .Bind(builder.Configuration.GetSection(AutomationOptions.SectionName));
    builder.Services.AddSingleton<IValidateOptions<AutomationOptions>, AutomationOptionsValidator>();

    // The API key is attached once, here, as a default header - it is never logged.
    builder.Services.AddHttpClient<IReportApiClient, ReportApiClient>((serviceProvider, client) =>
    {
        var api = serviceProvider.GetRequiredService<IOptions<AutomationOptions>>().Value.Api;
        client.BaseAddress = new Uri(api.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(api.TimeoutSeconds);
        client.DefaultRequestHeaders.Add(ApiKeyHeader.Name, api.Key);
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
