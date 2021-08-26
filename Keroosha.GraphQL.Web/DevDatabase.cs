using Keroosha.GraphQL.Web.Auth;
using Keroosha.GraphQL.Web.Database;
using Keroosha.GraphQL.Web.Models;
using Keroosha.GraphQL.Web.Models.Repositories;
using LinqToDB.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Keroosha.GraphQL.Web
{
    public class DevDatabase
    {
        private readonly ILogger<DevDatabase> _logger;
        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly AppDbContextManager _manager;
        private readonly IConfiguration _configuration;

        public DevDatabase(
            ILogger<DevDatabase> logger,
            IUserRepository users,
            IRoleRepository roles,
            AppDbContextManager manager,
            IConfiguration configuration)
        {
            _logger = logger;
            _users = users;
            _roles = roles;
            _manager = manager;
            _configuration = configuration;
        }

        public void ClearAndPopulateDatabaseWithSampleData(DatabaseType type, bool dropSchema = true)
        {
            if (dropSchema)
            {
                _logger.LogInformation("Dropping public schema of the database...");
                _manager.Exec(context =>
                {
                    context.Execute("DROP SCHEMA IF EXISTS public CASCADE;");
                    context.Execute("CREATE SCHEMA public;");
                });

                _logger.LogInformation("Migrating the database...");
                var connectionString = _configuration["Database:ConnectionString"];
                MigrationRunner.MigrateDb(connectionString, typeof(Startup).Assembly, type);
            }

            _logger.LogInformation("Creating the default user...");
            var id = _users.Create("user@example.com", "John Doe", PasswordToolkit.EncodeSshaPassword("123"));
            var user = _users.GetById(id);
            user.Confirmed = true;
            _users.Update(user);

            _roles.AttachRole(user.Id, Roles.Admin);
            _logger.LogInformation("Successfully managed to seed the database.");
        }
    }
}