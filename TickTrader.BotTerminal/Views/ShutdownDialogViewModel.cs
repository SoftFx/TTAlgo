using Caliburn.Micro;
using NLog;
using System;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class ShutdownDialogViewModel : Screen
    {
        public const int Delay = 500;

        public static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(120);
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly LocalAlgoAgent2 _algoAgent;
        private readonly Task _shutdownTask;

        private int _totalBots;
        private int _stoppedBots;


        public int TotalBots
        {
            get { return _totalBots; }
            set
            {
                _totalBots = value;
                NotifyOfPropertyChange();
            }
        }

        public int StoppedBots
        {
            get { return _stoppedBots; }
            set
            {
                _stoppedBots = value;
                NotifyOfPropertyChange();
            }
        }


        public ShutdownDialogViewModel(LocalAlgoAgent2 algoAgent)
        {
            _algoAgent = algoAgent;

            DisplayName = $"Shutting down - {EnvService.Instance.ApplicationName}";
            TotalBots = _algoAgent.RunningBotsCnt;
            StoppedBots = 0;
            _shutdownTask = algoAgent.Shutdown();
        }


        public async Task WaitShutdownBackground()
        {
            await Task.WhenAny(Task.Delay(WaitTimeout), _shutdownTask);

            if (!_shutdownTask.IsCompleted)
                _logger.Error("Wait timeout during algo server shutdown");
        }


        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            Task.Run(() => WaitBotsStop());
        }


        private async Task WaitBotsStop()
        {
            StoppedBots = TotalBots - _algoAgent.RunningBotsCnt;
            var waitTask = WaitShutdownBackground();
            while (!waitTask.IsCompleted)
            {
                await Task.Delay(Delay);
                StoppedBots = TotalBots - _algoAgent.RunningBotsCnt;
            }

            await TryCloseAsync();
        }
    }
}
