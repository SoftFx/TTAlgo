using Caliburn.Micro;
using SoftFX.Extended;
using SoftFX.Extended.Reports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Data;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class TradeHistoryViewModel : PropertyChangedBase
    {
        public enum TimePeriod { All, Today, Yesterday, CurrentMonth, PreviousMonth, LastThreeMonths, LastSixMonths, LastYear, Custom }
        public enum TradeDirection { All = 1, Buy, Sell }

        private ObservableSrotedList<string, TradeTransactionModel> _tradesList;
        private ObservableTask<TradeTransactionModel[]> _downloadObserver;
        private AccountModel _accountModel;
        private DateTime _from;
        private DateTime _to;
        private TimePeriod _period;
        private TradeDirection _tradeDirectionFilter;

        public TradeHistoryViewModel(AccountModel accountModel)
        {
            _period = TimePeriod.CurrentMonth;
            UpdateDateTimePeriod();
            TradeDirectionFilter = TradeDirection.All;

            _tradesList = new ObservableSrotedList<string, TradeTransactionModel>();
            TradesList = CollectionViewSource.GetDefaultView(_tradesList);
            TradesList.Filter = new Predicate<object>(FilterTradesList);

            _accountModel = accountModel;
            _accountModel.AccountTypeChanged += AccountTypeChanged;
            //_accountModel.StateChanged += AccountStateChanged;
            _accountModel.TradeHistory.OnTradeReport += TradeTransactionReport;
        }

        public bool IsNetAccount { get; private set; }
        public bool IsCachAccount { get; private set; }
        public bool IsGrossAccount { get; private set; }

        public TradeDirection TradeDirectionFilter
        {
            get { return _tradeDirectionFilter; }
            set
            {
                if (_tradeDirectionFilter != value)
                {
                    _tradeDirectionFilter = value;
                    NotifyOfPropertyChange(nameof(TradeDirectionFilter));
                    RefreshCollection();
                }
            }
        }

        public TimePeriod Period
        {
            get { return _period; }
            set
            {
                if (_period != value)
                {
                    _period = value;
                    NotifyOfPropertyChange(nameof(Period));
                    UpdateDateTimePeriod();
                    LoadHistory();
                }
            }
        }

        public DateTime From
        {
            get { return _from; }
            set
            {
                if (_from == value)
                    return;

                _from = value;
                _period = TimePeriod.Custom;
                NotifyOfPropertyChange(nameof(From));
                NotifyOfPropertyChange(nameof(Period));
                LoadHistory();
            }
        }

        public DateTime To
        {
            get { return _to; }
            set
            {
                if (_to == value)
                    return;

                _to = value;
                _period = TimePeriod.Custom;
                NotifyOfPropertyChange(nameof(To));
                NotifyOfPropertyChange(nameof(Period));
                LoadHistory();
            }
        }

        public ICollectionView TradesList { get; private set; }

        public ObservableTask<TradeTransactionModel[]> DownloadObserver
        {
            get { return _downloadObserver; }
            set
            {
                if (_downloadObserver == value)
                    return;

                _downloadObserver = value;
                NotifyOfPropertyChange(nameof(DownloadObserver));
            }
        }

        public async void LoadHistory()
        {
            try
            {
                _tradesList.Clear();

                var downloadRequest = _accountModel.TradeHistory.DownloadHistoryAsync(From.ToUniversalTime(), To.ToUniversalTime());
                DownloadObserver = new ObservableTask<TradeTransactionModel[]>(downloadRequest);
                var trades = await DownloadObserver.Task;

                if (downloadRequest != DownloadObserver.Task)
                    return;

                UpdateTradeHistory(trades);
            }
            catch { }
        }

        public void Close()
        {
            _accountModel.AccountTypeChanged -= AccountTypeChanged;
            //_accountModel.State.StateChanged -= AccountStateChanged;
            _accountModel.TradeHistory.OnTradeReport -= TradeTransactionReport;
        }

        private void UpdateTradeHistory(TradeTransactionModel[] trades)
        {
            for (int i = 0; i < trades.Length; i++)
                AddIfNeed(trades[i]);
        }

        private void AddIfNeed(TradeTransactionModel tradeTransaction)
        {
            if (_tradesList.GetOrDefault(tradeTransaction.Id) == null)
                _tradesList.Add(tradeTransaction.Id, tradeTransaction);
        }

        private void UpdateDateTimePeriod()
        {
            switch (Period)
            {
                case TimePeriod.All:
                    _from = new DateTime(1980, 01, 01);
                    _to = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case TimePeriod.CurrentMonth:
                    _from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    _to = _from.AddMonths(1).AddSeconds(-1);
                    break;
                case TimePeriod.PreviousMonth:
                    _from = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
                    _to = _from.AddMonths(1).AddSeconds(-1);
                    break;
                case TimePeriod.Today:
                    _from = DateTime.Today;
                    _to = _from.AddDays(1).AddSeconds(-1);
                    break;
                case TimePeriod.Yesterday:
                    _from = DateTime.Today.AddDays(-1);
                    _to = _from.AddDays(1).AddSeconds(-1);
                    break;
                case TimePeriod.LastThreeMonths:
                    _from = DateTime.Today.AddMonths(-3);
                    _to = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case TimePeriod.LastSixMonths:
                    _from = DateTime.Today.AddMonths(-5);
                    _to = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case TimePeriod.LastYear:
                    _from = DateTime.Today.AddYears(-1);
                    _to = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
            }

            NotifyOfPropertyChange(nameof(From));
            NotifyOfPropertyChange(nameof(To));
        }

        private void RefreshCollection()
        {
            if (this.TradesList != null)
                TradesList.Refresh();
        }

        private bool FilterTradesList(object o)
        {
            if (o != null)
            {
                var tradeT = (TradeTransactionModel)o;
                return TradeDirectionFilter == TradeDirection.All || TradeDirectionFilter == Convert(tradeT.TradeRecordSide);
            }
            return false;
        }

        private TradeDirection Convert(TradeTransactionModel.TradeSide side)
        {
            switch (side)
            {
                case TradeTransactionModel.TradeSide.Buy: return TradeDirection.Buy;
                case TradeTransactionModel.TradeSide.Sell: return TradeDirection.Sell;
                default: return default(TradeDirection);
            }
        }

        private void TradeTransactionReport(TradeTransactionModel tradeTransaction)
        {
            if (tradeTransaction.TransactionTime.Between(From, To))
                Execute.OnUIThread(() => AddIfNeed(tradeTransaction));
        }

        //private void AccountStateChanged(AccountModel.States arg1, AccountModel.States arg2)
        //{
        //    if (_accountModel.State.Current == AccountModel.States.Online)
        //        LoadHistory();
        //}

        private void AccountTypeChanged()
        {
            IsCachAccount = _accountModel.Type == AccountType.Cash;
            IsGrossAccount = _accountModel.Type == AccountType.Gross;
            IsNetAccount = _accountModel.Type == AccountType.Net;

            NotifyOfPropertyChange(nameof(IsCachAccount));
            NotifyOfPropertyChange(nameof(IsGrossAccount));
            NotifyOfPropertyChange(nameof(IsNetAccount));
        }
    }
}
