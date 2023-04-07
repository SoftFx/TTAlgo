using CommunityToolkit.Mvvm.ComponentModel;

namespace TickTrader.Algo.Updater
{
    internal partial class UpdateWindowViewModel : ObservableObject
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
                _ctx.StatusUpdated += msg => Status = msg;

                _ = _ctx.RunUpdateAsync();
            }
        }
    }
}
