namespace TickTrader.BotAgent.Configurator
{
    public class SpinnerViewModel : BaseViewModel
    {
        private bool _run = false;

        public bool Run
        {
            get => _run;

            set
            {
                if (_run == value)
                    return;

                _run = value;
                OnPropertyChanged(nameof(Run));
            }
        }
    }
}
