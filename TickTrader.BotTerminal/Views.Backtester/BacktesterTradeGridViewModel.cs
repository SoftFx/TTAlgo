using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    class BacktesterTradeGridViewModel : Page
    {
        private ObservableCollection<BaseTransactionModel> _reports = new ObservableCollection<BaseTransactionModel>();
        private Dictionary<string, ISymbolInfo> _symbolMap = new Dictionary<string, ISymbolInfo>();
        private AccountInfo.Types.Type _accType = AccountInfo.Types.Type.Gross;
        private int _accDigits = 5;


        public TradeHistoryGridViewModel GridView { get; }

        public IReadOnlyList<BaseTransactionModel> Reports { get; private set; }


        public BacktesterTradeGridViewModel(ProfileManager profile = null)
        {
            DisplayName = "Trade History";

            GridView = new TradeHistoryGridViewModel(new List<BaseTransactionModel>(), profile, true);
            GridView.ConvertTimeToLocal = false;
            GridView.IsSlippageSupported = false;
            GridView.AccType.Value = _accType;
        }


        public void Clear()
        {
            _reports.Clear();
            Reports = _reports;
            GridView.SetCollection(_reports);
        }

        public void Init(BacktesterConfig config)
        {
            Clear();
            _accType = config.Account.Type;
            _accDigits = config.TradeServer.Currencies.Values.FirstOrDefault(c => c.Name == config.Account.BalanceCurrency)?.Digits ?? 5;
            GridView.AccType.Value = _accType;
        }

        public void OnStart(BacktesterConfig config)
        {
            Init(config);
            _symbolMap = config.TradeServer.Symbols.Values.ToDictionary(s => s.Name, v => (ISymbolInfo)v);
        }

        public void Append(TradeReportInfo record)
        {
            var report = BaseTransactionModel.Create(_accType, record, _accDigits, _symbolMap.GetOrDefault(record.Symbol));
            _reports.Add(report);
        }

        public async Task LoadTradeHistory(BacktesterResults results, BacktesterConfig config)
        {
            var tradeHistory = new List<BaseTransactionModel>(results.TradeHistory.Count);
            var accType = _accType;
            var accDigits = _accDigits;
            var symbolMap = config.TradeServer.Symbols.Values.ToDictionary(s => s.Name, v => (ISymbolInfo)v);

            await Task.Run(() =>
            {
                foreach (var record in results.TradeHistory)
                {
                    var trRep = BaseTransactionModel.Create(accType, record, accDigits, symbolMap.GetOrDefault(record.Symbol));
                    tradeHistory.Add(trRep);
                }
            });

            Reports = tradeHistory;
            GridView.SetCollection(tradeHistory);
        }
    }
}
