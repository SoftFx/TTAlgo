using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public ModelErrorCounter ErrorCounter { get; }

        public BaseViewModel()
        {
            ErrorCounter = new ModelErrorCounter();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public static string GetChangeMessage(string category, string oldVal, string newVal)
        {
            return $"{category} was changed: {oldVal} to {newVal}";
        }
    }
}
