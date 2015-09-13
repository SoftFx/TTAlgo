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
        private ConnectionModel connection;
        private TriggeredActivity updateActivity;
        private bool isBusy;

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
            NotifyOfPropertyChange("Data");

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
                NotifyOfPropertyChange("Data");
            }
            catch (Exception ex)
            {
            }

            this.IsBusy = false;
        }

        public OhlcDataSeries<DateTime, double> Data { get; private set; }
        public string Symbol { get; private set; }
    }
}
