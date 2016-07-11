using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class BotControlViewModel : PropertyChangedBase
    {
        private bool isStarted = false;
        private bool isEnabled = true;
        private PluginExecutor executor;

        public BotControlViewModel(AlgoPluginRef pRef, FeedModel feed)
        {
            BotName = pRef.DisplayName;
            executor = pRef.CreateExecutor();
            executor.FeedProvider = new PluginFeedProvider(feed.Symbols, feed.History);
            executor.FeedStrategy = new BarStrategy();
            executor.InvokeStrategy = new DataflowInvokeStartegy();
        }

        public TimeFrames TimeFrame { get { return executor.TimeFrame; } set { executor.TimeFrame = value; } }
        public string MainSymbol { get { return executor.MainSymbolCode; } set { executor.MainSymbolCode = value; } }
        public DateTime TimelineStart { get { return executor.TimePeriodStart; } set { executor.TimePeriodStart = value; } }
        public DateTime TimelineEnd { get { return executor.TimePeriodEnd; } set { executor.TimePeriodEnd = value; } }

        public async void StartStop()
        {
            if (IsStarted)
            {
                IsEnabled = false;
                await Task.Factory.StartNew(() => executor.Stop());
                IsEnabled = true;
                IsStarted = false;
            }
            else
            {
                IsEnabled = false;
                await Task.Factory.StartNew(() => executor.Start());
                IsEnabled = true;
                IsStarted = true;
            }
        }

        public void Close()
        {
            Closed(this);
        }

        public event Action<BotControlViewModel> Closed = delegate { };

        public string BotName { get; private set; }

        public bool CanBeClosed { get { return IsEnabled && !IsStarted; } }

        public bool IsStarted
        {
            get { return isStarted; }
            set
            {
                isStarted = value;
                NotifyOfPropertyChange(nameof(IsStarted));
                NotifyOfPropertyChange(nameof(CanBeClosed));
            }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                NotifyOfPropertyChange(nameof(IsEnabled));
                NotifyOfPropertyChange(nameof(CanBeClosed));
            }
        }
    }
}
