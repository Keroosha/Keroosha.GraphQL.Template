using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keroosha.GraphQL.Web.Auth;
using Keroosha.GraphQL.Web.Config;
using Keroosha.GraphQL.Web.Database;
using Keroosha.GraphQL.Web.GraphQL;
using Keroosha.GraphQL.Web.Managers;
using Keroosha.GraphQL.Web.Storages;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Keroosha.GraphQL.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        private DatabaseConfig DatabaseConfig => _configuration.GetSection("Database").Get<DatabaseConfig>();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AutoRegisterByTypeName();
            services.AddSingleton<UserAuthManager>();
            services.AddSingleton<BlobManager>();
            services.AddSingleton<DevDatabase>();
            services.AddGraphQLServer()
                .AddAuthorization()
                .AddQueryType<Query>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            var blobType = new BlobConfig();
            _configuration.GetSection("Blob").Bind(blobType);
            if (blobType.Type is BlobTypes.Local)
                services.AddSingleton<IBlobFileStorage>(new LocalBlobFileStorage(blobType.Local.Path));

            var tokenStorageConfig = new TokenConfiguration();
            _configuration.GetSection("TokenStorage").Bind(tokenStorageConfig);
            switch (tokenStorageConfig.Type)
            {
                case TokenStorageType.JWT:
                    services.AddSingleton<ITokenStorage, JwtTokenStorage>();
                    break;
                case TokenStorageType.InMemory:
                    services.AddSingleton<ITokenStorage, InMemoryTokenStorage>();
                    break;
            }

            services.Configure<DatabaseConfig>(_configuration.GetSection("Database"));
            services.Configure<TokenConfiguration>(_configuration.GetSection("TokenStorage"));
            services.Configure<BlobConfig>(_configuration.GetSection("Blob"));
            services.Configure<EmailOptions>(_configuration.GetSection("Email"));

            services.AddSingleton<IAppDbConnectionFactory, AppDbConnectionFactory>();
            services.AddSingleton<AppDbContextManager>();

            if (_configuration.GetSection("Email").Exists())
                services.AddSingleton<IEmailConfirmationService, EmailConfirmationService>();
            else services.AddSingleton<IEmailConfirmationService, MockConfirmationService>();

            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            var notFound = Encoding.UTF8.GetBytes("Not found");
            app.Use((context, next) =>
            {
                if (context.Request.Method.ToUpperInvariant() != "GET"
                    || context.Request.Path
                        .ToString()?
                        .Split('/')
                        .LastOrDefault()?
                        .IndexOf('.') >= 0)
                {
                    context.Response.StatusCode = 404;
                    return context.Response.Body.WriteAsync(notFound, 0, notFound.Length);
                }

                context.Request.Path = "/index.html";
                return next();
            });
            
            MigrationRunner.MigrateDb(DatabaseConfig.ConnectionString, typeof(Startup).Assembly, DatabaseConfig.Type);
            var useDevelopmentDatabase = _configuration.GetValue<bool>("dev:database");
            if (useDevelopmentDatabase)
                app.ApplicationServices.GetService<DevDatabase>()?
                    .ClearAndPopulateDatabaseWithSampleData(DatabaseConfig.Type);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AutoRegisterByTypeName(this IServiceCollection services)
        {
            var types = typeof(Startup).Assembly.GetTypes();
            var namedTypes = new Dictionary<string, Type>();
            types.ToList().ForEach(t => namedTypes[t.Name] = t);
            types.Where(t => t.IsInterface && t.Name.EndsWith("Repository"))
                .ToList()
                .ForEach(RegisterRepository);
            types.Where(t => t.IsInterface && t.Name.EndsWith("AppService"))
                .ToList()
                .ForEach(RegisterAppService);

            return services;

            void RegisterRepository(Type repoType) => services
                .AddSingleton(repoType, namedTypes["Sql" + repoType.Name[1..]]);

            void RegisterAppService(Type repoType) => services
                .AddSingleton(repoType, namedTypes[repoType.Name[1..]]);
        }
    }
}