using System;
using System.Diagnostics;
using System.Management;
using System.ServiceProcess;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class ServiceManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly TimeSpan ProcessCloseDelay;

        private ServiceController ServiceController => new ServiceController(MachineServiceName);

        public string ServiceDisplayName => ServiceController.DisplayName;

        public bool IsServiceRunning => ServiceController.Status == ServiceControllerStatus.Running;

        public string MachineServiceName { get; }


        public ServiceManager(string serviceName)
        {
            MachineServiceName = serviceName;
            ProcessCloseDelay = new TimeSpan(0, 0, 90);
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
                throw new Exception($"{Resources.CannotStartServiceEx} {MachineServiceName}");
            }
        }

        public void ServiceStop()
        {
            var service = ServiceController;
            var machineName = ServiceController.MachineName;

            if (service.Status == ServiceControllerStatus.Stopped)
                throw new Exception(Resources.StopServiceEx);

            try
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, ProcessCloseDelay);
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                _logger.Error(ex);

                var processId = GetProcessIdByServiceName(ServiceController.ServiceName);

                if (processId != 0/* && service.Status == ServiceControllerStatus.Running*/)
                {
                    _logger.Info($"The process {processId} will be killed");
                    Process.GetProcessById(processId).Kill();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                //throw new Exception($"{Resources.CannotStopServiceEx} {MachineServiceName}");
                throw ex;
            }
        }

        private static int GetProcessIdByServiceName(string serviceName)
        {
            var managementObjects = new ManagementObjectSearcher($"SELECT PROCESSID FROM WIN32_SERVICE WHERE Name = '{serviceName}'").Get();

            foreach (ManagementObject mngntObj in managementObjects)
                return (int)(uint)mngntObj["PROCESSID"];

            return 0;
        }
    }
}
