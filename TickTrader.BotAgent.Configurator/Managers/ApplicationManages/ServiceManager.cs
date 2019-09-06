﻿using System;
using System.Management;
using System.ServiceProcess;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class ServiceManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _serviceName;

        private ServiceController _serviceController;

        public string ServiceDisplayName => _serviceController.DisplayName;

        public bool IsServiceRunning => _serviceController?.Status == ServiceControllerStatus.Running;

        public ServiceControllerStatus ServiceStatus => _serviceController.Status;

        public int ServiceId { get; private set; }


        public ServiceManager(string serviceName)
        {
            _serviceName = serviceName;
            _serviceController = new ServiceController(_serviceName);
        }

        public void ServiceStart(int listeningPort)
        {
            _serviceController = new ServiceController(_serviceName);

            if (IsServiceRunning)
                ServiceStop();

            if (_serviceController.Status == ServiceControllerStatus.Running)
                throw new Exception(Resources.StartServiceEx);

            try
            {
                _serviceController.Start();
                _serviceController.WaitForStatus(ServiceControllerStatus.Running);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception($"{Resources.CannotStartServiceEx} {_serviceName}");
            }
        }

        public void ServiceStop()
        {
            _serviceController = new ServiceController(_serviceName);

            if (_serviceController.Status == ServiceControllerStatus.Stopped)
                throw new Exception(Resources.StopServiceEx);

            try
            {
                _serviceController.Stop();
                _serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw new Exception($"{Resources.CannotStopServiceEx} {_serviceName}");
            }
        }
    }
}
