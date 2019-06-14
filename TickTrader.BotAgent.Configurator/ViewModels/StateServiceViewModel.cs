using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ServiceProcess;

namespace TickTrader.BotAgent.Configurator
{
    public class StateServiceViewModel : INotifyPropertyChanged
    {
        private readonly string _serviceName;
        private string _state;

        public string ServiceState => $"{_serviceName} is {_state}";

        public ServiceControllerStatus Status { get; private set; }

        public StateServiceViewModel(string serviceName)
        {
            _serviceName = serviceName;
        }

        public void RefreshService(ServiceControllerStatus currentState)
        {
            _state = currentState.ToString();

            Status = currentState;

            OnPropertyChanged(nameof(ServiceState));
            OnPropertyChanged(nameof(Status));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
