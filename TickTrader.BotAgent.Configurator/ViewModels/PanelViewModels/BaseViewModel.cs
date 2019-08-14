using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public static void NotifyStaticPropertyChanged([CallerMemberName] string name = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public abstract class BaseContentViewModel : BaseViewModel, IDataErrorInfo, IContentViewModel
    {
        public string ModelDescription { get; set; }

        public ModelErrorCounter ErrorCounter { get; }

        public string Error { get; }

        public virtual string this[string columnName] => throw new System.NotImplementedException();

        public BaseContentViewModel(string key = "")
        {
            ErrorCounter = new ModelErrorCounter(key);
        }

        public static string GetChangeMessage(string category, string oldVal, string newVal)
        {
            return $"{category} was changed: {oldVal} to {newVal}";
        }

        public virtual void RefreshModel() { }
    }

    public interface IContentViewModel
    {
        ModelErrorCounter ErrorCounter { get; }

        string ModelDescription { get; set; }

        void RefreshModel();
    }
}
