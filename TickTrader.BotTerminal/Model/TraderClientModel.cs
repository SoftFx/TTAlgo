using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class TraderClientModel
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum States { Offline, Online }

        public TraderClientModel(ConnectionModel connection)
        {
            this.Connection = connection;

            connection.Initalizing += Connection_Initalizing;
            connection.Connected += Connection_Connected;
            connection.Disconnected += Connection_Disconnected;
            connection.Deinitalizing += Connection_Deinitalizing;

            this.Symbols = new SymbolCollectionModel(connection);
            this.History = new FeedHistoryProviderModel(connection);
            this.TradeApi = new TradeExecutor(this);
            this.Account = new AccountModel(this);
        }

        private void Connection_Connected()
        {
            try
            {
                Connected?.Invoke();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connection_Connected() failed.");
            }
        }

        private void Connection_Disconnected()
        {
            try
            {
                Disconnected?.Invoke();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connection_Disconnected() failed.");
            }
        }

        private async Task Connection_Initalizing(object sender, CancellationToken cancelToken)
        {
            try
            {
                var cache = Connection.FeedProxy.Cache;
                CurrencyList = cache.Currencies;
                Symbols.Initialize(cache.Symbols, cache.Currencies);
                Account.Init(cache.Currencies);
                if (Initializing != null)
                    await Initializing.InvokeAsync(this, cancelToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connection_Initalizing() failed.");
            }
        }

        private async Task Connection_Deinitalizing(object sender, CancellationToken cancelToken)
        {
            await Account.Deinit();
            if (Deinitializing != null)
                await Deinitializing.InvokeAsync(this, cancelToken);
        }

        public event AsyncEventHandler Initializing;
        public event Action Connected;
        public event AsyncEventHandler Deinitializing;
        public event Action Disconnected;

        public ConnectionModel Connection { get; private set; }
        public TradeExecutor TradeApi { get; private set; }
        public AccountModel Account { get; private set; }
        public SymbolCollectionModel Symbols { get; private set; }
        public FeedHistoryProviderModel History { get; private set; }
        public IEnumerable<CurrencyInfo> CurrencyList { get; private set; }
    }
}
