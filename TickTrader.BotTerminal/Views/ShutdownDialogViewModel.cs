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


        public static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(30);


        private ChartCollectionViewModel _charts;
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


        public ShutdownDialogViewModel(ChartCollectionViewModel charts)
        {
            _charts = charts;

            DisplayName = $"Shuting down - {EnvService.Instance.ApplicationName}";
            TotalBots = _charts.Items.Sum(c => c.Bots.Count(b => b.IsStarted));
            StoppedBots = 0;

            StopBots();
        }


        private async void StopBots()
        {
            _charts.Items.Foreach(c => c.StopBots());

            StoppedBots = _charts.Items.Sum(c => c.Bots.Count(b => b.IsStarted));
            var startTime = DateTime.Now;
            while (_charts.Items.Any(c => c.HasStartedBots) && DateTime.Now - startTime < WaitTimeout)
            {
                await Task.Delay(Delay);
                StoppedBots = _charts.Items.Sum(c => c.Bots.Count(b => b.IsStarted));
            }

            TryClose();
        }
    }
}
