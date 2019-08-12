﻿using System;
using System.Management;
using System.ServiceProcess;

namespace TickTrader.BotAgent.Configurator
{
    public class ServiceManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _serviceName;

        private ServiceController _serviceController;
        private int _serviceId;

        public bool IsServiceRunning => _serviceController?.Status == ServiceControllerStatus.Running;

        public ServiceControllerStatus ServiceStatus => _serviceController.Status;

        public int ServiceId
        {
            get => _serviceId;
            set
            {
                if (IsServiceRunning && _serviceId != value)
                    _serviceId = value;
            }
        }

        public ServiceManager(string serviceName)
        {
            _serviceName = serviceName;
            _serviceController = new ServiceController(_serviceName);

            GetServiceId();
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
                GetServiceId();
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

        public void GetServiceId()
        {
            if (!IsServiceRunning)
                return;

            try
            {
                var query = $"SELECT ProcessId FROM Win32_Service WHERE Name='{_serviceName}'";
                var searcher = new ManagementObjectSearcher(query);

                foreach (var obj in searcher.Get())
                {
                    _serviceId = (int)obj["ProcessId"];
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            };
        }
    }
}
