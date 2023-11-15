using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace TickTrader.Algo.Updater
{
    internal partial class UpdateWindowViewModel : ObservableObject, IUpdateObserver
    {
        private static UpdateContext _ctx;

        public static void Init(UpdateContext context) => _ctx = context;

        [ObservableProperty]
        private string _status;
        [ObservableProperty]
        private bool _isError;


        public UpdateWindowViewModel()
        {
            Status = "Loading...";

            if (_ctx != null) // Remove design-time error
            {
                _ctx.UpdateObserver = this;

                _ = _ctx.RunUpdateAsync();
            }
        }


        public void OnStatusUpdated(string msg) => Status = msg;

        public bool KillQuestionCallback(string msg)
        {
            var msgRes = MessageBox.Show(msg, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return msgRes == MessageBoxResult.Yes;
        }

        public void OnCompleted() => App.Current.Dispatcher.BeginInvoke(() => App.Current.Shutdown());
    }
}
