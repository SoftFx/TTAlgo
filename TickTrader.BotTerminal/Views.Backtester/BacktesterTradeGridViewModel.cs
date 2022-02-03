using System.Collections.Generic;
using System.Collections.ObjectModel;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    class BacktesterTradeGridViewModel : Page
    {
        private ObservableCollection<BaseTransactionModel> _reports = new ObservableCollection<BaseTransactionModel>();

        public BacktesterTradeGridViewModel(ProfileManager profile = null)
        {
            DisplayName = "Trade History";

            GridView = new TradeHistoryGridViewModel(new List<BaseTransactionModel>(), profile, true);
            GridView.ConvertTimeToLocal = false;
            GridView.IsSlippageSupported = false;
            GridView.AccType.Value = AccountInfo.Types.Type.Gross;
            GridView.SetCollection(_reports);
        }

        public TradeHistoryGridViewModel GridView { get; }

        public void OnTesterStart(AccountInfo.Types.Type newAccType)
        {
            _reports.Clear();
            GridView.AccType.Value = newAccType;
        }

        public void Append(BaseTransactionModel report)
        {
            _reports.Add(report);
        }
    }
}
