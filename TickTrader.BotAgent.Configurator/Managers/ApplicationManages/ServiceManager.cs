using System;
using System.Management;
using System.ServiceProcess;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class ServiceManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _serviceName;

        private ServiceController ServiceController => new ServiceController(_serviceName);

        public string ServiceDisplayName => ServiceController.DisplayName;

        public bool IsServiceRunning => ServiceController.Status == ServiceControllerStatus.Running;

        public ServiceManager(string serviceName)
        {
            _serviceName = serviceName;
        }

        public void ServiceStart(int listeningPort)
        {
            if (IsServiceRunning)
                ServiceStop();

            var service = ServiceController;

            if (service.Status == ServiceControllerStatus.Running)
                throw new Exception(Resources.StartServiceEx);

            try
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception($"{Resources.CannotStartServiceEx} {_serviceName}");
            }
        }

        public void ServiceStop()
        {
            var service = ServiceController;

            if (service.Status == ServiceControllerStatus.Stopped)
                throw new Exception(Resources.StopServiceEx);

            try
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception($"{Resources.CannotStopServiceEx} {_serviceName}");
            }
        }
    }
}
