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
        public const int Delay = 1000;


        public static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(60);


        private BotManager _botManager;
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


        public ShutdownDialogViewModel(BotManager botManager)
        {
            _botManager = botManager;

            DisplayName = $"Shuting down - {EnvService.Instance.ApplicationName}";
            TotalBots = _botManager.RunningBotsCnt;
            StoppedBots = 0;

            StopBots();
        }


        private async void StopBots()
        {
            _botManager.StopBots();

            StoppedBots = TotalBots - _botManager.RunningBotsCnt;
            var startTime = DateTime.Now;
            while (_botManager.HasRunningBots && DateTime.Now - startTime < WaitTimeout)
            {
                await Task.Delay(Delay);
                StoppedBots = TotalBots - _botManager.RunningBotsCnt;
            }

            TryClose();
        }
    }
}
