using System;
using System.ServiceProcess;

namespace TickTrader.BotAgent.Configurator
{
    public class ServiceManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private ServiceController _serviceController;
        private readonly string _serviceName;
        private int _servicePort;

        public bool IsServiceRunning => _serviceController?.Status == ServiceControllerStatus.Running;

        public ServiceControllerStatus ServiceStatus => _serviceController.Status;
        public int ServicePort
        {
            get => _servicePort;
            set
            {
                if (IsServiceRunning && _servicePort != value)
                    _servicePort = value;
            }
        }

        public ServiceManager(string serviceName)
        {
            _serviceName = serviceName;
            _serviceController = new ServiceController(_serviceName);
        }

        public void ServiceStart(int listeningPort)
        {
            _serviceController = new ServiceController(_serviceName);

            if (_serviceController.Status == ServiceControllerStatus.Running)
                throw new Exception("Service alredy started");

            try
            {
                _serviceController.Start();
                _serviceController.WaitForStatus(ServiceControllerStatus.Running);
                ServicePort = listeningPort;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception($"Cannot start Windows Service {_serviceName}");
            }
        }

        public void ServiceStop()
        {
            if (_serviceController.Status == ServiceControllerStatus.Stopped)
                throw new Exception("Service alredy stopped");

            try
            {
                _serviceController.Stop();
                _serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception($"Cannot stopped Windows Service {_serviceName}");
            }
        }
    }
}
