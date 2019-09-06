using System.ServiceProcess;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class StateServiceViewModel : BaseViewModel
    {
        private string _serviceName, _infoMessage;
        private bool _visibleRestartMessage;

        private RefreshCounter _refresh;

        public delegate void ChangeServiceStatus();

        public event ChangeServiceStatus ChangeServiceStatusEvent;

        public string RestartMessage => Resources.ApplyNewSettingMes;

        public string ServiceState => $"{_serviceName} is {Status.ToString()}";

        public bool ServiceRunning => Status == ServiceControllerStatus.Running;

        public ServiceControllerStatus Status { get; private set; }

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

        public void RefreshService(string serviceName)
        {
            _serviceName = serviceName;

            var oldStatus = Status;

            Status = new ServiceController(_serviceName).Status;
            VisibleRestartMessage = ServiceRunning ? _refresh.Restart : false;

            if (oldStatus != Status)
            {
                InfoMessage = ServiceRunning ? Resources.StartAgentLog : Resources.StopAgentLog;

                if (ServiceRunning)
                    _refresh.DropRestart();
            }

            ChangeServiceStatusEvent?.Invoke();
            OnPropertyChanged(nameof(ServiceState));
            OnPropertyChanged(nameof(ServiceRunning));
            OnPropertyChanged(nameof(Status));
        }
    }
}
