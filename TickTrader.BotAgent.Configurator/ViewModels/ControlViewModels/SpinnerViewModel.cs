namespace TickTrader.BotAgent.Configurator
{
    public class SpinnerViewModel : BaseViewModel
    {
        public bool Run { get; private set; }

        public void Start()
        {
            Run = true;
            OnPropertyChanged(nameof(Run));
        }

        public void Stop()
        {
            Run = false;
            OnPropertyChanged(nameof(Run));
        }
    }
}
