using System;
using System.Linq;
using System.Threading.Tasks;
using Keroosha.GraphQL.Web.Config;
using Keroosha.GraphQL.Web.Models;
using FluentMigrator.Exceptions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.DataProvider.SqlServer;
using Microsoft.Extensions.Options;

namespace Keroosha.GraphQL.Web.Database
{
    public class AppDbContext : DataConnection
    {
        public AppDbContext(string connectionString, IDataProvider provider) : base(provider, connectionString)
        {
        }

        public ITable<User> Users => GetTable<User>();
        public ITable<UserRole> UserRoles => GetTable<UserRole>();
        public ITable<Blob> Blobs => GetTable<Blob>();
    }

    public interface IAppDbConnectionFactory
    {
        public AppDbContext GetConnection();
    }

    public class AppDbConnectionFactory : IAppDbConnectionFactory
    {
        private readonly IOptions<DatabaseConfig> _config;

        public AppDbConnectionFactory(IOptions<DatabaseConfig> config)
        {
            _config = config;
        }

        public AppDbContext GetConnection()
        {
            return _config.Value.Type switch
            {
                DatabaseType.SqlServer => new AppDbContext(ConnectionString(), MssqlProvider()),
                DatabaseType.Pgsql => new AppDbContext(ConnectionString(), PgsqlProvider()),
                _ => throw new DatabaseOperationNotSupportedException(
                    $"{nameof(AppDbConnectionFactory)} can't find available db type connection")
            };
        }

        private static SqlServerDataProvider MssqlProvider()
        {
            return new
                ("app", SqlServerVersion.v2017, SqlServerProvider.MicrosoftDataSqlClient);
        }

        private static PostgreSQLDataProvider PgsqlProvider()
        {
            return new();
        }

        private string ConnectionString()
        {
            return _config.Value.ConnectionString;
        }
    }

    public class AppDbContextManager : DbContextManagerBase<AppDbContext>
    {
        public AppDbContextManager(IAppDbConnectionFactory factory) : base(factory.GetConnection)
        {
        }
    }

    public class DbContextManagerBase<TContext> where TContext : DataConnection
    {
        private readonly Func<TContext> _factory;

        public DbContextManagerBase(Func<TContext> factory)
        {
            _factory = factory;
        }

        public void Exec(Action<TContext> cb)
        {
            using (var ctx = _factory())
            {
                cb(ctx);
            }
        }

        public T Exec<T>(Func<TContext, T> cb)
        {
            using (var ctx = _factory())
            {
                var rv = cb(ctx);
                if (rv is IQueryable)
                    throw new InvalidOperationException("IQueryable leak detected");
                return rv;
            }
        }

        public async Task<T> ExecAsync<T>(Func<TContext, Task<T>> cb)
        {
            using (var ctx = _factory())
            {
                return await cb(ctx);
            }
        }

        public async Task ExecAsync(Func<TContext, Task> cb)
        {
            using (var ctx = _factory())
            {
                await cb(ctx);
            }
        }
    }
}