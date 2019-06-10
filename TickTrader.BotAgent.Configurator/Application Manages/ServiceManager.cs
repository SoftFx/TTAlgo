using System;
using System.Linq;
using System.ServiceProcess;

namespace TickTrader.BotAgent.Configurator
{
    public class ServiceManager
    {
        private ServiceController _serviceController;
        private readonly string _serviceName;

        public bool IsServiceRunning => _serviceController?.Status == ServiceControllerStatus.Running;


        public ServiceManager(string serviceName)
        {
            _serviceName = serviceName;
            _serviceController = new ServiceController(_serviceName);
        }

        public void ServiceStart()
        {
            if (_serviceController.Status == ServiceControllerStatus.Running)
                throw new Exception("Service alredy started");

            try
            {
                _serviceController.Start();
                _serviceController.WaitForStatus(ServiceControllerStatus.Running);
            }
            catch
            {
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
            catch
            {
                throw new Exception($"Cannot stopped Windows Service {_serviceName}");
            }
        }
    }
}
