using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public abstract class BaseViewModelWithValidations : BaseViewModel, IDataErrorInfo
    {
        public string Error { get; }

        public virtual string this[string columnName] => throw new System.NotImplementedException();
    }

    public abstract class BaseContentViewModel : BaseViewModelWithValidations, IContentViewModel
    {
        public ModelErrorCounter ErrorCounter { get; }

        public string ModelDescription { get; set; }

        public BaseContentViewModel(string key = "")
        {
            ErrorCounter = new ModelErrorCounter(key);
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
