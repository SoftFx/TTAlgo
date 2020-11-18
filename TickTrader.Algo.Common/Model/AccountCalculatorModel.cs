using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;
using TickTrader.Common.Business;

namespace TickTrader.Algo.Common.Model
{
    public abstract class AccountCalculatorModel
    {
        protected static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<AccountCalculatorModel>();

        private IMarketDataProvider _client;
        private IFeedSubscription _subscription;

        protected AccountCalculatorModel() { }

        private AccountCalculatorModel(AccountModel acc, IMarketDataProvider client)
        {
            _client = client;
            Account = new AccountAdapter(acc);
            MarketModel = new MarketState(NettingCalculationTypes.OneByOne);

            MarketModel.Set(client.Symbols.Snapshot.Values.OrderBy(s => s.Descriptor.GroupSortOrder).ThenBy(s => s.Descriptor.SortOrder).ThenBy(s => s.Descriptor.Name));
            MarketModel.Set(client.Currencies.Snapshot.Values.Select(c => new CurrencyInfoAdapter(c)));

            _subscription = client.Distributor.AddSubscription(Symbols_RateUpdated, client.Symbols.Snapshot.Keys);

            foreach (var smb in client.Symbols.Snapshot.Values)
            {
                if (smb.LastQuote != null)
                    MarketModel.Update(smb.LastQuote);
            }

            //_subscription.NewQuote += ;
        }

        protected AccountAdapter Account { get; private set; }
        protected MarketState MarketModel { get; private set; }

        public decimal Equity { get; protected set; }
        public decimal Margin { get; protected set; }
        public decimal Profit { get; protected set; }
        public decimal Floating { get; protected set; }
        public decimal MarginLevel { get; protected set; }
        public decimal Swap { get; protected set; }

        public event Action<AccountCalculatorModel> Updated;

        public virtual void Recalculate() { }

        protected void OnUpdate()
        {
            Updated?.Invoke(this);
        }

        public static AccountCalculatorModel Create(AccountModel acc, IMarketDataProvider client)
        {
            try
            {
                switch (acc.Type)
                {
                    case Api.AccountTypes.Cash:
                        return new CashCalc(acc, client);
                    case Api.AccountTypes.Net:
                    case Api.AccountTypes.Gross:
                        return new MarginCalc(acc, client);
                    default:
                        throw new Exception("Acc type is null");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to init Account Calculator", ex);
            }
            return new NullCalc();
        }

        public virtual void Dispose()
        {
            Account.Dispose();
            _subscription.CancelAll();
        }

        private void Symbols_RateUpdated(QuoteEntity quote)
        {
            MarketModel.Update(quote);
        }

        //private class RateAdapter : ISymbolRate
        //{
        //    public RateAdapter(SoftFX.Extended.Quote quote)
        //    {
        //        //if (quote.Symbol == "ETHRUB")
        //        //{
        //        //    System.Diagnostics.Debug.WriteLine("ETHRUB!111111 ololo!");
        //        //}

        //        if (quote.HasAsk)
        //        {
        //            Ask = (decimal)quote.Ask;
        //            NullableAsk = (decimal)quote.Ask;
        //        }

        //        if (quote.HasBid)
        //        {
        //            Bid = (decimal)quote.Bid;
        //            NullableBid = (decimal)quote.Bid;
        //        }

        //        Symbol = quote.Symbol;
        //    }

        //    public decimal Ask { get; private set; }
        //    public decimal Bid { get; private set; }
        //    public decimal? NullableAsk { get; private set; }
        //    public decimal? NullableBid { get; private set; }
        //    public string Symbol { get; private set; }
        //}

        private class CurrencyInfoAdapter : ICurrencyInfo
        {
            public CurrencyInfoAdapter(CurrencyEntity info)
            {
                Name = info.Name;
                Precision = info.Precision;
                SortOrder = info.SortOrder;
            }

            public string Name { get; private set; }
            public int Precision { get; private set; }
            public int SortOrder { get; private set; }
            public CurrencyType Type => CurrencyType.Default;
        }

        protected class AccountAdapter : IMarginAccountInfo, ICashAccountInfo
        {
            private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<AccountAdapter>();

            private AccountModel acc;

            public AccountAdapter(AccountModel acc)
            {
                this.acc = acc;
                this.acc.Orders.Updated += Orders_Updated;
                this.acc.Positions.Updated += Positions_Updated;
                this.acc.Assets.Updated += Assets_Updated;
            }

            public void Dispose()
            {
                this.acc.Orders.Updated -= Orders_Updated;
                this.acc.Positions.Updated -= Positions_Updated;
                this.acc.Assets.Updated -= Assets_Updated;
            }

            public AccountingTypes AccountingType
            {
                get
                {
                    switch (acc.Type)
                    {
                        case Api.AccountTypes.Cash: return AccountingTypes.Cash;
                        case Api.AccountTypes.Gross: return AccountingTypes.Gross;
                        case Api.AccountTypes.Net: return AccountingTypes.Net;
                        default: throw new NotImplementedException("Account type is not supported: " + acc.Type); //sometime error after change acc type
                    }
                }
            }

            public decimal Balance { get { return (decimal)acc.Balance; } }
            public string BalanceCurrency { get { return acc.BalanceCurrency; } }
            public long Id { get { return 0; } }
            public int Leverage { get { return acc.Leverage; } }

            public IEnumerable<IOrderModel> Orders { get { return acc.Orders.Snapshot.Values; } }
            public IEnumerable<IPositionModel> Positions { get { return acc.Positions.Snapshot.Values; } }
            public IEnumerable<IAssetModel> Assets { get { return acc.Assets.Snapshot.Values; } }

            private void Orders_Updated(DictionaryUpdateArgs<string, OrderModel> args)
            {
                try
                {
                    if (args.Action == DLinqAction.Insert)
                        OrderAdded(args.NewItem);
                    else if (args.Action == DLinqAction.Replace)
                        OrderReplaced(args.NewItem);
                    else if (args.Action == DLinqAction.Remove)
                        OrderRemoved(args.OldItem);
                }
                catch(Exception ex)
                {
                    logger.Error(ex, "Order update failed.");
                }
            }

            private void Positions_Updated(DictionaryUpdateArgs<string, PositionModel> args)
            {
                try
                {
                    if (args.Action == DLinqAction.Insert || args.Action == DLinqAction.Replace)
                        PositionChanged(args.NewItem, PositionChageTypes.AddedModified);
                    else if (args.Action == DLinqAction.Remove)
                        PositionChanged(args.OldItem, PositionChageTypes.Removed);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Position update failed.");
                }
            }

            private void Assets_Updated(DictionaryUpdateArgs<string, AssetModel> args)
            {
                try
                {
                    if (args.Action == DLinqAction.Insert)
                        AssetsChanged(args.NewItem, AssetChangeTypes.Added);
                    else if (args.Action == DLinqAction.Replace)
                        AssetsChanged(args.NewItem, AssetChangeTypes.Replaced);
                    else if (args.Action == DLinqAction.Remove)
                        AssetsChanged(args.OldItem, AssetChangeTypes.Removed);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Asset update failed.");
                }
            }

            public void LogInfo(string message)
            {
            }

            public void LogWarn(string message)
            {
            }

            public void LogError(string message)
            {
            }

            public event Action<IOrderModel> OrderAdded = delegate { };
            public event Action<IOrderModel> OrderRemoved = delegate { };
            public event Action<IOrderModel> OrderReplaced = delegate { };
            public event Action<IEnumerable<IOrderModel>> OrdersAdded = delegate { };
            public event Action<IPositionModel, PositionChageTypes> PositionChanged = delegate { };
            public event Action<IAssetModel, AssetChangeTypes> AssetsChanged = delegate { };
        }

        private class MarginCalc : AccountCalculatorModel
        {
            private AccCalcAdapter calc;

            public MarginCalc(AccountModel acc, IMarketDataProvider client)
                : base(acc, client)
            {
                calc = new AccCalcAdapter(Account,  MarketModel);
                calc.Updated = () =>
                {
                    Equity = calc.Equity;
                    Margin = calc.Margin;
                    Profit = calc.Profit;
                    Floating = calc.Profit + calc.Commission + calc.Swap;
                    MarginLevel = calc.MarginLevel;
                    Swap = calc.Swap;
                    OnUpdate();
                };
            }

            public override void Recalculate()
            {
                calc.UpdateSummary(UpdateKind.AccountBalanceChanged);
            }
        }

        private class CashCalc : AccountCalculatorModel
        {
            private CashAccountCalculator calc;

            public CashCalc(AccountModel acc, IMarketDataProvider client)
                : base(acc, client)
            {
                this.calc = new CashAccountCalculator(Account, MarketModel);
            }
        }

        private class NullCalc : AccountCalculatorModel
        {
            public NullCalc()
            {
                Equity = -1;
                Margin = -1;
                Profit = -1;
                Floating = -1;
                MarginLevel = -1;
                Swap = -1;
            }

            public override void Dispose() { }
        }

        private class AccCalcAdapter : AccountCalculator
        {
            public AccCalcAdapter(IMarginAccountInfo infoProvider, MarketState market) : base(infoProvider, market)
            {
            }

            public Action Updated { get; set; }

            protected override void OnUpdated()
            {
                Updated?.Invoke();
            }
        }
    }
}
