using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    class ChartViewModel : Screen
    {
        private readonly ConnectionModel connection;
        private readonly TriggeredActivity updateActivity;

        private static readonly List<KeyValuePair<string, BarPeriod>> PredefinedPeriods = new List<KeyValuePair<string,BarPeriod>>();

        static ChartViewModel()
        {
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("Monthly", BarPeriod.MN1));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("Weekly", BarPeriod.W1));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("Daily", BarPeriod.D1));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("4 hours", BarPeriod.H4));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("1 hour", BarPeriod.H1));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("30 minutes", BarPeriod.M30));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("15 minutes", BarPeriod.M15));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("5 minutes", BarPeriod.M5));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("1 minute", BarPeriod.M1));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("10 seconds", BarPeriod.S10));
            PredefinedPeriods.Add(new KeyValuePair<string, BarPeriod>("1 second", BarPeriod.S1));
        }


        public ChartViewModel(string symbol, ConnectionModel connection)
        {
            this.Symbol = symbol;
            this.DisplayName = symbol;
            this.connection = connection;

            this.updateActivity = new TriggeredActivity(Update);

            connection.Connected += connection_Connected;
            connection.Disconnected += connection_Disconnected;

            if (connection.State.Current == ConnectionModel.States.Online)
                updateActivity.Trigger();
        }

        #region Bindable Properties

        private bool isBusy;
        private IndexRange visibleRange = new IndexRange(0, 10);
        private OhlcDataSeries<DateTime, double> data;

        public IEnumerable<KeyValuePair<string, BarPeriod>> AvailablePeriods { get { return PredefinedPeriods; } }

        public IndexRange VisibleRange
        {
            get { return visibleRange; }
            set
            {
                visibleRange = value;
                NotifyOfPropertyChange("VisibleRange");
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    NotifyOfPropertyChange("IsBusy");
                }
            }
        }

        public OhlcDataSeries<DateTime, double> Data
        {
            get { return data; }
            set
            {
                data = value;
                NotifyOfPropertyChange("Data");
            }
        }

        #endregion

        private Task connection_Disconnected(object sender)
        {
            return updateActivity.Abort();
        }

        private void connection_Connected()
        {
            updateActivity.Trigger(true);
        }

        private async Task Update(CancellationToken cToken)
        {
            this.Data = null;
            this.IsBusy = true;

            try
            {
                var response = await Task.Factory.StartNew(
                    () => connection.FeedProxy.Server.GetHistoryBars(
                        Symbol, DateTime.Now + TimeSpan.FromDays(1),
                        -4000, SoftFX.Extended.PriceType.Ask, BarPeriod.H1));

                cToken.ThrowIfCancellationRequested();

                var newData = new OhlcDataSeries<DateTime, double>();

                foreach (var bar in response.Bars.Reverse())
                    newData.Append(bar.From, bar.Open, bar.High, bar.Low, bar.Close);

                
                this.Data = newData;
                if (newData.Count > 0)
                {
                    this.VisibleRange.Max = newData.Count - 1;
                    this.VisibleRange.Min = Math.Max(0, newData.Count - 101);
                }
            }
            catch (Exception ex)
            {
            }

            this.IsBusy = false;
        }

        public string Symbol { get; private set; }
    }
}