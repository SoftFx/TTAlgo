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
        private LocalAlgoAgent2 _agent;
        private DockManagerService _dockManagerService;


        public ProfileLoadingDialogViewModel(ChartCollectionViewModel charts, ProfileManager profileManager, CancellationToken token,
            LocalAlgoAgent2 agent, DockManagerService dockManagerService)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _charts = charts;
            _profileManager = profileManager;
            _token = token;
            _agent = agent;
            _dockManagerService = dockManagerService;
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            _ = ApplyProfile();

            return base.OnInitializeAsync(cancellationToken);
        }

        private async Task ApplyProfile()
        {
            try
            {
                await Task.Delay(Delay, _token); //give UI some time to display this window

                await _agent.IndicatorHost.Stop();

                _charts.CloseAllItems(_token);
                _dockManagerService.RemoveViews();

                _token.ThrowIfCancellationRequested();

                //await _agent.AlgoServer.PkgStorage.WaitLoaded();

                _token.ThrowIfCancellationRequested();

                if (_profileManager.CurrentProfile.Charts == null)
                {
                    await TryCloseAsync();
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

            await TryCloseAsync();
        }
    }
}
