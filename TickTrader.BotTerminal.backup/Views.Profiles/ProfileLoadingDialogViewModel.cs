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
        private LocalAlgoAgent _agent;
        private DockManagerService _dockManagerService;


        public ProfileLoadingDialogViewModel(ChartCollectionViewModel charts, ProfileManager profileManager, CancellationToken token,
            LocalAlgoAgent agent, DockManagerService dockManagerService)
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
            ApplyProfile();

            return base.OnInitializeAsync(cancellationToken);
        }

        //protected override void OnInitialize()
        //{
        //    base.OnInitialize();

        //    ApplyProfile();
        //}

        private async void ApplyProfile()
        {
            try
            {
                await Task.Delay(Delay, _token); //give UI some time to display this window

                _charts.CloseAllItems(_token);
                _dockManagerService.RemoveViews();
                _agent.RemoveAllBots(_token);

                _token.ThrowIfCancellationRequested();

                await _agent.Library.WaitInit();

                _token.ThrowIfCancellationRequested();

                _agent.LoadBotsSnapshot(_profileManager.CurrentProfile, _token);

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
