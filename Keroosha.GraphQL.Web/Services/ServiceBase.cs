using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace Keroosha.GraphQL.Web.Services
{
    public abstract class ServiceBase
    {
        private readonly AsyncLock _lock = new();

        public static TimeSpan? IntervalOverride { get; set; }
        protected TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(15);

        public void Start(IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger(GetType());
            var task = Task.Run(async () =>
            {
                while (!lifetime.ApplicationStopping.IsCancellationRequested)
                    try
                    {
                        using (await _lock.LockAsync(lifetime.ApplicationStopping))
                        {
                            await Run(lifetime.ApplicationStopping);
                        }

                        await Task.Delay(IntervalOverride ?? Interval, lifetime.ApplicationStopping);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in a service");
                        await Task.Delay(IntervalOverride ?? TimeSpan.FromMinutes(1), lifetime.ApplicationStopping);
                    }
            });
            lifetime.ApplicationStopped.Register(() => task.Wait());
        }

        public async Task ForceSync(CancellationToken token)
        {
            using (await _lock.LockAsync(token))
            {
                await Run(token);
            }
        }

        protected abstract Task Run(CancellationToken token);
    }

    public static class ServiceRunner
    {
        private static readonly List<Type> ServiceTypes;

        static ServiceRunner()
        {
            ServiceTypes = typeof(ServiceBase).Assembly.GetTypes()
                .Where(typeof(ServiceBase).IsAssignableFrom)
                .Where(t => !t.IsAbstract).ToList();
        }

        public static void RegisterServices(IServiceCollection serviceCollection)
        {
            foreach (var t in ServiceTypes)
                serviceCollection.AddSingleton(t);
        }

        public static void StartServices(IServiceProvider provider)
        {
            var lifetime = provider.GetRequiredService<IHostApplicationLifetime>();
            var logger = provider.GetRequiredService<ILoggerFactory>();
            foreach (var st in ServiceTypes)
            {
                var service = (ServiceBase)provider.GetService(st);
                service.Start(lifetime, logger);
            }
        }

        public static Task SyncAllServices(IServiceProvider serviceProvider, CancellationToken token)
        {
            return Task.WhenAll(ServiceTypes
                .Select(s => Task.Run(() => ((ServiceBase)serviceProvider.GetService(s)).ForceSync(token))).ToArray());
        }
    }
}