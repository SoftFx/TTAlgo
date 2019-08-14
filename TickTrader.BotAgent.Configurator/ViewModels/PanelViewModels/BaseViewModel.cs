using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo, IContentViewModel
    {
        public string ModelDescription { get; set; }

        public ModelErrorCounter ErrorCounter { get; }

        public string Error { get; }

        public virtual string this[string columnName] => throw new System.NotImplementedException();

        public BaseViewModel(string key = "")
        {
            ErrorCounter = new ModelErrorCounter(key);
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

        public virtual void RefreshModel() { }
    }
}
