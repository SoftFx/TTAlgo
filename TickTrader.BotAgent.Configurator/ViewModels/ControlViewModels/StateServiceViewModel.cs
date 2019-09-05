using System.ServiceProcess;

namespace TickTrader.BotAgent.Configurator
{
    public class StateServiceViewModel : BaseViewModel
    {
        private string _serviceName, _infoMessage;
        private bool _visibleRestartMessage;

        public delegate void ChangeServiceStatus();

        public event ChangeServiceStatus StopServiceEvent;
        public event ChangeServiceStatus StartServiceEvent;

        public string RestartMessage => "To apply the new settings, restart the service.";

        public string ServiceState => $"{_serviceName} is {Status.ToString()}";

        public ServiceControllerStatus Status { get; private set; }

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

            Status = new ServiceController(_serviceName).Status;

            if (Status == ServiceControllerStatus.Stopped)
                StopServiceEvent?.Invoke();
            else
            if (Status == ServiceControllerStatus.Running)
                StartServiceEvent?.Invoke();

            OnPropertyChanged(nameof(ServiceState));
            OnPropertyChanged(nameof(Status));
        }
    }
}
