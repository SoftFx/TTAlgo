using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.BusinessLogic;
using TickTrader.BusinessObjects;
using TickTrader.Common.Business;

namespace TickTrader.Algo.Common.Model
{
    public abstract class AccountCalculatorModel
    {
        private ClientCore _client;
        private IFeedSubscription _subscription;

        private AccountCalculatorModel(AccountModel acc, ClientCore client)
        {
            _client = client;
            Account = new AccountAdapter(acc);
            MarketModel = new MarketState(NettingCalculationTypes.OneByOne);

            MarketModel.Set(client.Symbols.Snapshot.Values);
            MarketModel.Set(client.Currencies.Snapshot.Values.Select(c => new CurrencyInfoAdapter(c)));

            _subscription = client.Distributor.SubscribeAll();

            foreach (var smb in client.Symbols.Snapshot.Values)
            {
                if (smb.LastQuote != null)
                    MarketModel.Update(new RateAdapter(smb.LastQuote));
            }

            _subscription.NewQuote += Symbols_RateUpdated;
        }

        protected AccountAdapter Account { get; private set; }
        protected MarketState MarketModel { get; private set; }

        public decimal Equity { get; protected set; }
        public decimal Margin { get; protected set; }
        public decimal Profit { get; protected set; }
        public decimal Floating { get; protected set; }
        public decimal MarginLevel { get; protected set; }

        public event Action<AccountCalculatorModel> Updated;

        public virtual void Recalculate() { }

        protected void OnUpdate()
        {
            Updated?.Invoke(this);
        }

        public static AccountCalculatorModel Create(AccountModel acc, ClientCore client)
        {
            if (acc.Type == SoftFX.Extended.AccountType.Cash)
                return new CashCalc(acc, client);
            else
                return new MarginCalc(acc, client);
        }

        public virtual void Dispose()
        {
            Account.Dispose();
            _subscription.Dispose();
        }

        private void Symbols_RateUpdated(SoftFX.Extended.Quote quote)
        {
            MarketModel.Update(new RateAdapter(quote));
        }

        private class RateAdapter : ISymbolRate
        {
            public RateAdapter(SoftFX.Extended.Quote quote)
            {
                //if (quote.Symbol == "ETHRUB")
                //{
                //    System.Diagnostics.Debug.WriteLine("ETHRUB!111111 ololo!");
                //}

                if (quote.HasAsk)
                {
                    Ask = (decimal)quote.Ask;
                    NullableAsk = (decimal)quote.Ask;
                }

                if (quote.HasBid)
                {
                    Bid = (decimal)quote.Bid;
                    NullableBid = (decimal)quote.Bid;
                }

                Symbol = quote.Symbol;
            }

            public decimal Ask { get; private set; }
            public decimal Bid { get; private set; }
            public decimal? NullableAsk { get; private set; }
            public decimal? NullableBid { get; private set; }
            public string Symbol { get; private set; }
        }

        private class CurrencyInfoAdapter : ICurrencyInfo
        {
            public CurrencyInfoAdapter(SoftFX.Extended.CurrencyInfo info)
            {
                Name = info.Name;
                Precision = info.Precision;
                SortOrder = info.SortOrder;
            }

            public string Name { get; private set; }
            public int Precision { get; private set; }
            public int SortOrder { get; private set; }
        }

        protected class AccountAdapter : IMarginAccountInfo, ICashAccountInfo
        {
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
                        case SoftFX.Extended.AccountType.Cash: return AccountingTypes.Cash;
                        case SoftFX.Extended.AccountType.Gross: return AccountingTypes.Gross;
                        case SoftFX.Extended.AccountType.Net: return AccountingTypes.Net;
                        default: throw new NotImplementedException("Account type is not supported: " + acc.Type);
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
                if (args.Action == DLinqAction.Insert)
                    OrderAdded(args.NewItem);
                else if (args.Action == DLinqAction.Replace)
                    OrderReplaced(args.NewItem);
                else if (args.Action == DLinqAction.Remove)
                    OrderRemoved(args.OldItem);
            }

            private void Positions_Updated(DictionaryUpdateArgs<string, PositionModel> args)
            {
                if (args.Action == DLinqAction.Insert || args.Action == DLinqAction.Replace)
                    PositionChanged(args.NewItem, PositionChageTypes.AddedModified);
                else if (args.Action == DLinqAction.Remove)
                    PositionChanged(args.OldItem, PositionChageTypes.Removed);
            }

            private void Assets_Updated(DictionaryUpdateArgs<string, AssetModel> args)
            {
                if (args.Action == DLinqAction.Insert)
                    AssetsChanged(args.NewItem, AssetChangeTypes.Added);
                else if (args.Action == DLinqAction.Replace)
                    AssetsChanged(args.NewItem, AssetChangeTypes.Replaced);
                else if(args.Action == DLinqAction.Remove)
                    AssetsChanged(args.OldItem, AssetChangeTypes.Removed);
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

            public MarginCalc(AccountModel acc, ClientCore client)
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

            public CashCalc(AccountModel acc, ClientCore client)
                : base(acc, client)
            {
                this.calc = new CashAccountCalculator(Account, MarketModel);
            }
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
