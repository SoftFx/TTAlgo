using Caliburn.Micro;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    class ProfileLoadingDialogViewModel : Screen
    {
        public const int Delay = 100;


        private Logger _logger;
        private ChartCollectionViewModel _charts;
        private ProfileManager _profileManager;
        private CancellationToken _token;
        private LocalAlgoLibrary _library;
        private BotManagerViewModel _botManager;
        private DockManagerService _dockManagerService;


        public ProfileLoadingDialogViewModel(ChartCollectionViewModel charts, ProfileManager profileManager, CancellationToken token,
            LocalAlgoLibrary library, BotManagerViewModel botManager, DockManagerService dockManagerService)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _charts = charts;
            _profileManager = profileManager;
            _token = token;
            _library = library;
            _botManager = botManager;
            _dockManagerService = dockManagerService;
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

                await _library.WaitInit();

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

                _token.ThrowIfCancellationRequested();

                _dockManagerService.LoadLayoutSnapshot(_profileManager.CurrentProfile);
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
