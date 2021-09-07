using System;
using System.ServiceProcess;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class StateServiceViewModel : BaseViewModel
    {
        private readonly RefreshCounter _refresh;

        private string _displayServiceName, _infoMessage;
        private bool _visibleRestartMessage;

        public string RestartMessage => Resources.ApplyNewSettingMes;

        public string ServiceState => $"{_displayServiceName} is {Status}";

        public bool ServiceRunning => Status == ServiceControllerStatus.Running;

        public ServiceControllerStatus Status { get; private set; }

        public bool RestartRequired { get; set; }


        public event Action ChangeServiceStatus;


        public StateServiceViewModel(RefreshCounter refresh)
        {
            _refresh = refresh;
        }

        public string InfoMessage
        {
            get => _infoMessage;
            set
            {
                if (_infoMessage == value)
                    return;

                _infoMessage = value;

                OnPropertyChanged(nameof(InfoMessage));
            }
        }

        public bool VisibleRestartMessage
        {
            get => _visibleRestartMessage;
            set
            {
                if (_visibleRestartMessage == value)
                    return;

                _visibleRestartMessage = value;

                OnPropertyChanged(nameof(VisibleRestartMessage));
            }
        }

        public void RefreshService(string machineName, string displayServiceName)
        {
            _displayServiceName = displayServiceName;

            var oldStatus = Status;

            Status = new ServiceController(machineName).Status;
            VisibleRestartMessage = ServiceRunning && (_refresh.Restart || RestartRequired);

            if (oldStatus != Status)
            {
                if (oldStatus != 0)
                {
                    InfoMessage = ServiceRunning ? Resources.StartAgentLog : Resources.StopAgentLog;

                    if (ServiceRunning)
                    {
                        _refresh.DropRestart();
                        RestartRequired = false;
                    }
                }

                ChangeServiceStatus?.Invoke();
            }

            OnPropertyChanged(nameof(ServiceState));
            OnPropertyChanged(nameof(ServiceRunning));
            OnPropertyChanged(nameof(Status));
        }
    }
}
