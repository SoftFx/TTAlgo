using System.ServiceProcess;

namespace TickTrader.BotAgent.Configurator
{
    public class StateServiceViewModel : BaseViewModel
    {
        private string _serviceName, _infoMessage;
        private bool _visibleRestartMessage;

        private RefreshCounter _refresh;

        public delegate void ChangeServiceStatus();

        public event ChangeServiceStatus ChangeServiceStatusEvent;

        public string RestartMessage => "To apply the new settings, restart the service.";

        public string ServiceState => $"{_serviceName} is {Status.ToString()}";

        public bool ServiceRun => Status == ServiceControllerStatus.Running;

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

            Status = new ServiceController(_serviceName).Status;

            VisibleRestartMessage = ServiceRun ? _refresh.Update : false;

            ChangeServiceStatusEvent?.Invoke();
            OnPropertyChanged(nameof(ServiceState));
            OnPropertyChanged(nameof(Status));
        }
    }
}
