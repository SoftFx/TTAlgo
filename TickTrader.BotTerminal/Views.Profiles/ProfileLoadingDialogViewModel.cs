using Caliburn.Micro;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class ProfileLoadingDialogViewModel : Screen
    {
        public const int Delay = 100;


        private Logger _logger;
        private ChartCollectionViewModel _charts;
        private ProfileManager _profileManager;
        private CancellationToken _token;


        public ProfileLoadingDialogViewModel(ChartCollectionViewModel charts, ProfileManager profileManager, CancellationToken token)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _charts = charts;
            _profileManager = profileManager;
            _token = token;
        }


        protected override void OnInitialize()
        {
            base.OnInitialize();

            ApplyProfile();
        }


        private async void ApplyProfile()
        {
            try
            {
                await Task.Delay(Delay, _token);

                _charts.CloseAllItems(_token);

                _token.ThrowIfCancellationRequested();

                if (_profileManager.CurrentProfile.Charts == null)
                {
                    _profileManager.CurrentProfile.Save();
                    TryClose();
                    return;
                }

                while (_charts.Items.Count > 0)
                {
                    _token.ThrowIfCancellationRequested();
                    await Task.Delay(Delay, _token);
                }

                _charts.LoadProfile(_profileManager.CurrentProfile, _token);
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to apply profile");
            }

            TryClose();
        }
    }
}
