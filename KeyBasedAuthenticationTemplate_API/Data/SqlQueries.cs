using System.Collections.Concurrent;
using System.Reflection;

namespace KeyBasedAuthenticationTemplate_API.Data
{
    // Loads the .sql files under /Sql, which are compiled into the assembly as embedded resources
    // (see the csproj). Embedded means they ship inside the dll - no loose files to go missing or be
    // edited on a server. Names are "<Folder>.<FileWithoutExtension>", e.g. "Postgres.ProductsAll".
    public static class SqlQueries
    {
        private static readonly Assembly Assembly = typeof(SqlQueries).Assembly;
        private static readonly ConcurrentDictionary<string, string> Cache = new();

        public static string Load(string name) => Cache.GetOrAdd(name, ReadResource);

        private static string ReadResource(string name)
        {
            // Embedded resource names are "<RootNamespace>.<folder path with dots>.<file>"; the root
            // namespace is the assembly name in this project.
            var resourceName = $"{Assembly.GetName().Name}.Sql.{name}.sql";
            using var stream = Assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"SQL resource '{resourceName}' was not found. Is the file under /Sql and marked as an EmbeddedResource?");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
