using System;
using System.Diagnostics;
using System.Management;
using System.ServiceProcess;
using System.Threading.Tasks;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class ServiceManager
    {
        private const int DelayKillProcess = 5 * 60 * 1000;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly TimeSpan ServiceCloseDelay;

        private ServiceController ServiceController => new ServiceController(MachineServiceName);

        public string ServiceDisplayName => ServiceController.DisplayName;

        public bool IsServiceRunning => ServiceController.Status == ServiceControllerStatus.Running;

        public string MachineServiceName { get; }


        public ServiceManager(string serviceName)
        {
            MachineServiceName = serviceName;
            ServiceCloseDelay = new TimeSpan(0, 0, 90);
        }

        public async Task ServiceStart(int listeningPort)
        {
            if (IsServiceRunning)
            {
                ServiceStop();
                await Task.Delay(5000);
            }

            var service = ServiceController;

            if (service.Status == ServiceControllerStatus.Running)
                throw new Exception(Resources.StartServiceEx);

            try
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
            //catch (Exception ex)
            //{
            //    _logger.Error(ex);
            //    //throw new Exception($"{Resources.CannotStartServiceEx} {MachineServiceName}");
            //    throw ex;
            //}
            finally { }
        }

        public void ServiceStop()
        {
            var service = ServiceController;

            if (service.Status == ServiceControllerStatus.Stopped)
                throw new Exception(Resources.StopServiceEx);

            try
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, ServiceCloseDelay);
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                _logger.Error(ex);

                var processId = GetProcessIdByServiceName(ServiceController.ServiceName);

                if (processId != 0/* && service.Status == ServiceControllerStatus.Running*/)
                {
                    _logger.Info($"The process {processId} will be killed");
                    var process = Process.GetProcessById(processId);

                    process.Kill();
                    process.WaitForExit(DelayKillProcess);
                }
            }
            //catch (Exception ex)
            //{
            //    _logger.Error(ex);
            //    //throw new Exception($"{Resources.CannotStopServiceEx} {MachineServiceName}");
            //    throw ex;
            //}
            finally { }
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
