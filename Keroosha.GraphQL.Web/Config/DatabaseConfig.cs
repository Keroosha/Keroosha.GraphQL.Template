using Keroosha.GraphQL.Web.Database;

namespace Keroosha.GraphQL.Web.Config
{
    public class DatabaseConfig
    {
        public DatabaseType Type { get; set; }
        public string ConnectionString { get; set; }
    }
}