using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.ComponentModel;
using System.ServiceProcess;

namespace TickTrader.BotAgent
{
    /// <summary>
    /// Wrapper around windows service to resolve logs not being saved on system shutdown
    /// Dotnet runtime WindowsServiceLifetime seems to release ServiceBase.OnStop() too early and system goes off before flushing logs to disk
    /// Adapted version of https://github.com/dotnet/aspnetcore/blob/main/src/Hosting/WindowsServices/src/WebHostService.cs
    /// </summary>
    [DesignerCategory("Code")]
    public class GenericHostService : ServiceBase
    {
        private readonly IHost _host;
        private bool _stopRequestedByWindows;


        public GenericHostService(IHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));

            CanShutdown = true;
        }


        public static void Run(IHost host)
        {
            var service = new GenericHostService(host);
            ServiceBase.Run(service);
        }


        protected sealed override void OnStart(string[] args)
        {
            OnStarting(args);

            _host.Start();

            OnStarted();

            // Register callback for application stopping after we've
            // started the service, because otherwise we might introduce unwanted
            // race conditions.
            _host
                .Services
                .GetRequiredService<IHostApplicationLifetime>()
                .ApplicationStopping
                .Register(() =>
                {
                    if (!_stopRequestedByWindows)
                    {
                        Stop();
                    }
                });
        }

        protected sealed override void OnStop()
        {
            _stopRequestedByWindows = true;
            OnStopping();
            try
            {
                _host.StopAsync().GetAwaiter().GetResult();
            }
            finally
            {
                _host.Dispose();
                OnStopped();
            }
        }

        protected sealed override void OnShutdown() => OnStop();


        protected virtual void OnStarting(string[] args) { }

        protected virtual void OnStarted() { }

        protected virtual void OnStopping() { }

        protected virtual void OnStopped()
        {
            NLog.LogManager.Flush();
            NLog.LogManager.Shutdown();
        }
    }
}
