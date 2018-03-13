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
        private PluginCatalog _repo;
        private BotManagerViewModel _botManager;


        public ProfileLoadingDialogViewModel(ChartCollectionViewModel charts, ProfileManager profileManager, CancellationToken token,
            PluginCatalog repo, BotManagerViewModel botManager)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _charts = charts;
            _profileManager = profileManager;
            _token = token;
            _repo = repo;
            _botManager = botManager;
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
                await Task.Delay(Delay, _token); //give UI some time to display this window

                _charts.CloseAllItems(_token);
                _botManager.CloseAllBots(_token);

                _token.ThrowIfCancellationRequested();

                await _repo.WaitInit();

                _token.ThrowIfCancellationRequested();

                _botManager.LoadBotsSnapshot(_profileManager.CurrentProfile, _token);

                if (_profileManager.CurrentProfile.Charts == null)
                {
                    TryClose();
                    return;
                }

                while (_charts.Items.Count > 0)
                {
                    _token.ThrowIfCancellationRequested();
                    await Task.Delay(Delay, _token);
                }

                _charts.LoadChartsSnaphot(_profileManager.CurrentProfile, _token);
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
