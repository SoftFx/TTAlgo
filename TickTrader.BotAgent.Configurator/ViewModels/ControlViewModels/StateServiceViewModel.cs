using System.ServiceProcess;

namespace TickTrader.BotAgent.Configurator
{
    public class StateServiceViewModel : BaseViewModel
    {
        private readonly string _serviceName;

        private bool _visibleRestartMessage;
        private string _state;

        public string RestartMessage => "To apply the new settings, restart the service.";

        public string ServiceState => $"{_serviceName} is {_state}";

        public ServiceControllerStatus Status { get; private set; }

        public StateServiceViewModel(string serviceName)
        {
            _serviceName = serviceName;
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

        public void RefreshService(ServiceControllerStatus currentState)
        {
            _state = currentState.ToString();

            Status = currentState;

            OnPropertyChanged(nameof(ServiceState));
            OnPropertyChanged(nameof(Status));
        }
    }
}
