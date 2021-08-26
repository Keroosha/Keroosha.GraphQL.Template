using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Keroosha.GraphQL.Web.Cmdlets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Keroosha.GraphQL.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (CmdletManager.IsCommand(args))
            {
                var (res, host) = await CmdletManager.Execute(CreateHostBuilder, args);
                if (res is not 0) return;
                await host.RunAsync();
            }
            else
                await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IWebHostBuilder CreateHostBuilder(string[] args) =>
            new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration((hb, cb) =>
                {
                    cb.Sources.Clear();
                    cb.AddJsonFile("config.defaults.json")
                        .AddJsonFile("config.local.json", true)
                        .AddJsonFile("/data/config.json", true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .UseStartup<Startup>()
                .UseKestrel();
    }
}