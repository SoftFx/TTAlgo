using Caliburn.Micro;
using Machinarium.State;
using NLog;
using SoftFX.Extended;
using SoftFX.Extended.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Api;
using Machinarium.Qnil;
using System.Diagnostics;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal class AccountModel : Algo.Common.Model.AccountModel
    {
        private Logger logger;
        private TraderClientModel clientModel;
        private ConnectionModel connection;
        private ActionBlock<System.Action> uiUpdater;

        public AccountModel(TraderClientModel clientModel)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            this.clientModel = clientModel;
            this.connection = clientModel.Connection;
            TradeHistory = new TradeHistoryProvider(clientModel);

            clientModel.IsConnectingChanged += UpdateConnectingState;

            connection.Connecting += () =>
            {
                connection.TradeProxy.AccountInfo += AccountInfoChanged;
                connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
            };

            connection.Disconnecting += () =>
            {
                connection.TradeProxy.AccountInfo -= AccountInfoChanged;
                connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport -= TradeProxy_TradeTransactionReport;
                connection.TradeProxy.BalanceOperation -= TradeProxy_BalanceOperation;
            };
        }

        private void UpdateConnectingState()
        {
            //if (clientModel.IsConnecting)
            //{
            //    positions.Clear();
            //    orders.Clear();
            //    assets.Clear();
            //}
        }

        private Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            return Deinit();
        }

        public ConnectionModel Connection { get { return connection; } }
        public TradeHistoryProvider TradeHistory { get; private set; }
        public AccountCalculatorModel Calc { get; private set; }

        public void Init(IDictionary<string, CurrencyInfo> currencies)
        {
            Action<System.Action> uiActionhandler = (a) =>
            {
                try
                {
                    a();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Ui Action failed.");
                }
            };

            try
            {
                this.uiUpdater = DataflowHelper.CreateUiActionBlock<System.Action>(uiActionhandler, 100, 100, CancellationToken.None);
                UpdateData(currencies);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Init() failed.");
            }
        }

        private void UpdateData(IDictionary<string, CurrencyInfo> currencies)
        {
            var accInfo = connection.TradeProxy.Cache.AccountInfo;
            var balanceCurrencyInfo = currencies.GetOrDefault(accInfo.Currency);
            var cache = connection.TradeProxy.Cache;

            base.Init(accInfo, currencies, clientModel.Symbols, cache.TradeRecords, cache.Positions, cache.AccountInfo.Assets);

            Calc = AccountCalculatorModel.Create(this, clientModel);
            Calc.Recalculate();
        }

        public async Task Deinit()
        {
            Calc.Dispose();

            await Task.Factory.StartNew(() =>
                {
                    uiUpdater.Complete();
                    uiUpdater.Completion.Wait();
                });
        }

        private void TradeProxy_PositionReport(object sender, SoftFX.Extended.Events.PositionReportEventArgs e)
        {
            // TO DO: save updates in a buffer and apply them on Init()
            var uiUpdaterCopy = this.uiUpdater;
            if (uiUpdaterCopy == null)
                return;

            uiUpdater.SendAsync(() => OnReport(e.Report));
        }

        private void TradeProxy_ExecutionReport(object sender, SoftFX.Extended.Events.ExecutionReportEventArgs e)
        {
            uiUpdater.SendAsync(() => OnReport(e.Report));
        }

        private void TradeProxy_BalanceOperation(object sender, SoftFX.Extended.Events.NotificationEventArgs<BalanceOperation> e)
        {
            uiUpdater.SendAsync(() => OnBalanceOperation(sender, e));
        }

        private void TradeProxy_TradeTransactionReport(object sender, SoftFX.Extended.Events.TradeTransactionReportEventArgs e)
        {
            uiUpdater.SendAsync(() => OnTransactionReport(e.Report));
        }

        void AccountInfoChanged(object sender, SoftFX.Extended.Events.AccountInfoEventArgs e)
        {
            //Type = e.Information.Type;
        }

        public override void SyncInvoke(System.Action syncAction)
        {
            Caliburn.Micro.Execute.OnUIThread(syncAction);
        }

        protected override void OnBalanceChanged()
        {
            Calc.Recalculate();
        }
    }
}
