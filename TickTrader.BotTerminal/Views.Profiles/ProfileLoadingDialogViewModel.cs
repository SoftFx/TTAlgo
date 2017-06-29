using Caliburn.Micro;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class ProfileLoadingDialogViewModel : Screen
    {
        public const int Delay = 100;


        private ChartCollectionViewModel _charts;
        private ProfileManager _profileManager;


        public ProfileLoadingDialogViewModel(ChartCollectionViewModel charts, ProfileManager profileManager)
        {
            _charts = charts;
            _profileManager = profileManager;
        }


        protected override void OnInitialize()
        {
            base.OnInitialize();

            ApplyProfile();
        }


        private async void ApplyProfile()
        {
            await Task.Delay(1000);

            if (_profileManager.CurrentProfile.Charts == null)
            {
                _profileManager.CurrentProfile.Save();
                TryClose();
                return;
            }

            _charts.CloseAllItems();

            while (_charts.Items.Count > 0)
            {
                await Task.Delay(Delay);
            }

            _charts.LoadProfile(_profileManager.CurrentProfile);

            TryClose();
        }
    }
}
