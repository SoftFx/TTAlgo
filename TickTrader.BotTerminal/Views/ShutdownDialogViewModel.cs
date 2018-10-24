using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class ShutdownDialogViewModel : Screen
    {
        public const int Delay = 500;


        public static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(60);


        private LocalAlgoAgent _algoAgent;
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


        public ShutdownDialogViewModel(LocalAlgoAgent algoAgent)
        {
            _algoAgent = algoAgent;

            DisplayName = $"Shutting down - {EnvService.Instance.ApplicationName}";
            TotalBots = _algoAgent.RunningBotsCnt;
            StoppedBots = 0;
        }


        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            StopBots();
        }


        private async void StopBots()
        {
            _algoAgent.StopBots();

            StoppedBots = TotalBots - _algoAgent.RunningBotsCnt;
            var startTime = DateTime.Now;
            while (_algoAgent.HasRunningBots && DateTime.Now - startTime < WaitTimeout)
            {
                await Task.Delay(Delay);
                StoppedBots = TotalBots - _algoAgent.RunningBotsCnt;
            }

            TryClose();
        }
    }
}
