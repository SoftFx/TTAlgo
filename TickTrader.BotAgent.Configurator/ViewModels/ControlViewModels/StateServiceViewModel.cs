using System.ServiceProcess;

namespace TickTrader.BotAgent.Configurator
{
    public class StateServiceViewModel : BaseViewModel
    {
        private string _serviceName;

        private bool _visibleRestartMessage;

        public string RestartMessage => "To apply the new settings, restart the service.";

        public string ServiceState => $"{_serviceName} is {Status.ToString()}";

        public ServiceControllerStatus Status { get; private set; }

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

        public void RefreshService(string serviceName, ServiceControllerStatus currentState)
        {
            _serviceName = serviceName;

            Status = currentState;

            OnPropertyChanged(nameof(ServiceState));
            OnPropertyChanged(nameof(Status));
        }
    }
}
