using ActorSharp.Lib;
using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
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
using System.Windows.Threading;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class TradeHistoryViewModel : PropertyChangedBase
    {
        private const int CleanUpDelay = 2000;

        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum TimePeriod { All, LastHour, Today, Yesterday, CurrentMonth, PreviousMonth, LastThreeMonths, LastSixMonths, LastYear, Custom }
        public enum TradeDirection { All = 1, Buy, Sell }

        private ObservableSrotedList<string, TransactionReport> _tradesList;
        private ObservableTask<int> _downloadObserver;
        private TraderClientModel _tradeClient;
        private DateTime _from;
        private DateTime _to;
        private DateTime? _currenFrom;
        private DateTime? _currenTo;
        private bool _currentSkipCanceled;
        private TimePeriod _period;
        private bool _isRefreshing;
        private bool _isConditionsChanged;
        private TradeDirection _tradeDirectionFilter;
        private bool _skipCancel;
        private CancellationTokenSource _cancelUpdateSrc;

        public TradeHistoryViewModel(TraderClientModel tradeClient)
        {
            _period = TimePeriod.LastHour;
            TradeDirectionFilter = TradeDirection.All;
            _skipCancel = true;

            _tradesList = new ObservableSrotedList<string, TransactionReport>();
            TradesList = CollectionViewSource.GetDefaultView(_tradesList);
            TradesList.Filter = new Predicate<object>(FilterTradesList);

            _tradeClient = tradeClient;
            _tradeClient.Account.AccountTypeChanged += AccountTypeChanged;
            _tradeClient.TradeHistory.OnTradeReport += OnReport;

            tradeClient.Connected += RefreshHistory;

            RefreshHistory();
            CleanupLoop();
        }

        #region Properties
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
                    NotifyOfPropertyChange(nameof(CanEditPeriod));
                    RefreshHistory();
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
                NotifyOfPropertyChange(nameof(From));
                RefreshHistory();
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
                NotifyOfPropertyChange(nameof(To));
                RefreshHistory();
            }
        }

        public bool SkipCancel
        {
            get { return _skipCancel; }
            set
            {
                if (_skipCancel == value)
                    return;

                _skipCancel = value;
                NotifyOfPropertyChange(nameof(SkipCancel));
                RefreshHistory();
            }
        }

        public ICollectionView TradesList { get; private set; }

        public ObservableTask<int> DownloadObserver
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

        public bool CanEditPeriod => Period == TimePeriod.Custom;
        #endregion

        private async void RefreshHistory()
        {
            _isConditionsChanged = true;

            if (_isRefreshing)
            {
                _cancelUpdateSrc.Cancel();
                return;
            }

            while (_isConditionsChanged)
            {
                _isConditionsChanged = false;
                await AdjustHistoryBoundaries();
            }
        }

        private async Task AdjustHistoryBoundaries()
        {
            if (!_tradeClient.IsConnected.Value)
                return;

            DateTime? newFrom, newTo;
            CalculateTimeBoundaries(out newFrom, out newTo);

            if (newFrom == _currenFrom && newTo == _currenTo && _currentSkipCanceled == SkipCancel)
                return;

            _cancelUpdateSrc = new CancellationTokenSource();

            _isRefreshing = true;

            bool truncate = newFrom != null && newFrom >= _currenFrom && newTo == _currenTo && _currentSkipCanceled == SkipCancel;

            _currenFrom = newFrom;
            _currenTo = newTo;
            _currentSkipCanceled = SkipCancel;

            if (truncate)
                await TruncateHistoryAsync(newFrom.Value, _cancelUpdateSrc.Token);
            else
                await DownloadHistory(newFrom, newTo, _cancelUpdateSrc.Token);

            _isRefreshing = false;
        }

        private async Task DownloadHistory(DateTime? from, DateTime? to, CancellationToken cToken)
        {
            try
            {
                _tradesList.Clear();

                var downloadTask = DownloadingHistoryAsync(from?.ToUniversalTime(), to?.ToUniversalTime(), SkipCancel, cToken);
                DownloadObserver = new ObservableTask<int>(downloadTask);

                await downloadTask;
            }
            catch (TaskCanceledException) { logger.Debug("Load task canceled"); }
            catch (OperationCanceledException) { logger.Debug("Load operation canceled"); }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load trade history!");
            }
        }

        private async Task<int> DownloadingHistoryAsync(DateTime? from, DateTime? to, bool skipCancel, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return 0;

            var historyStream = _tradeClient.TradeHistory.GetTradeHistory(from, to, skipCancel);

            while (await historyStream.ReadNext())
            {
                if (token.IsCancellationRequested)
                {
                    await historyStream.Close();
                    break;
                }

                var report = historyStream.Current;
                var historyItem = CreateReportModel(report);
                AddToList(historyItem);
            }

            return 0;
        }

        private TransactionReport CreateReportModel(TradeReportEntity tTransaction)
        {
            return CreateReportModel(_tradeClient.Account.Type.Value, tTransaction, GetSymbolFor(tTransaction));
        }

        private static TransactionReport CreateReportModel(AccountTypes accountType, TradeReportEntity tTransaction, SymbolModel symbol = null)
        {
            switch (accountType)
            {
                case AccountTypes.Gross: return new GrossTransactionModel(tTransaction, symbol);
                case AccountTypes.Net: return new NetTransactionModel(tTransaction, symbol);
                case AccountTypes.Cash: return new CashTransactionModel(tTransaction, symbol);
                default: throw new NotSupportedException(accountType.ToString());
            }
        }

        private SymbolModel GetSymbolFor(TradeReportEntity transaction)
        {
            SymbolModel symbolModel = null;
            if (!IsBalanceOperation(transaction))
            {
                symbolModel = _tradeClient.Symbols.GetOrDefault(transaction.Symbol);

                if (symbolModel == null)
                    logger.Warn("Symbol {0} not found for TradeTransactionID {1}.", transaction.Symbol, transaction.Id);
            }

            return symbolModel;
        }

        private bool IsBalanceOperation(TradeReportEntity item)
        {
            return item.TradeTransactionReportType == TradeExecActions.BalanceTransaction;
        }

        public void Close()
        {
            _tradeClient.Account.AccountTypeChanged -= AccountTypeChanged;
            _tradeClient.Connected -= RefreshHistory;
            _tradeClient.TradeHistory.OnTradeReport -= OnReport;
        }

        public string GetTransactionKey(TransactionReport tradeTransaction)
        {
            return $"{tradeTransaction.CloseTime.Ticks} {tradeTransaction.UniqueId}";
        }

        private void AddToList(TransactionReport tradeTransaction)
        {
            try
            {
                var key = GetTransactionKey(tradeTransaction);
                _tradesList.Add(key, tradeTransaction);
            }
            catch (ArgumentException) { } // shit happens
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private bool MatchesCurrentBoundaries(DateTime reportTime)
        {
            var localTime = reportTime.ToLocalTime();

            return (_currenFrom == null || localTime >= _currenFrom)
                && (_currenTo == null || localTime < _currenTo);
        }

        private void CalculateTimeBoundaries(out DateTime? from, out DateTime? to)
        {
            var now = DateTime.Now; // fix time point

            switch (Period)
            {
                case TimePeriod.Custom:
                    from = From;
                    to = To;
                    break;
                case TimePeriod.All:
                    from = null;
                    to = null;
                    break;
                case TimePeriod.LastHour:
                    from = now - TimeSpan.FromHours(1);
                    to = null;
                    break;
                case TimePeriod.CurrentMonth:
                    from = new DateTime(now.Year, now.Month, 1);
                    to = null;
                    break;
                case TimePeriod.PreviousMonth:
                    from = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                    to = new DateTime(now.Year, now.Month, 1);
                    break;
                case TimePeriod.Today:
                    from = now.Date;
                    to = null;
                    break;
                case TimePeriod.Yesterday:
                    from = now.Date.AddDays(-1);
                    to = now.Date;
                    break;
                case TimePeriod.LastThreeMonths:
                    from = now.Date.AddMonths(-3);
                    to = null;
                    break;
                case TimePeriod.LastSixMonths:
                    from = now.Date.AddMonths(-6);
                    to = null;
                    break;
                case TimePeriod.LastYear:
                    from = now.Date.AddYears(-1);
                    to = null;
                    break;
                default: throw new Exception("Unsupported time period!");
            }
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
                var tradeT = (TransactionReport)o;
                return TradeDirectionFilter == TradeDirection.All || TradeDirectionFilter == Convert(tradeT.Side);
            }
            return false;
        }

        private TradeDirection Convert(TransactionReport.TransactionSide side)
        {
            switch (side)
            {
                case TransactionReport.TransactionSide.Buy: return TradeDirection.Buy;
                case TransactionReport.TransactionSide.Sell: return TradeDirection.Sell;
                default: return TradeDirection.All;
            }
        }

        private void OnReport(TradeReportEntity tradeTransaction)
        {
            if (MatchesCurrentBoundaries(tradeTransaction.CloseTime))
                AddToList(CreateReportModel(tradeTransaction));
        }

        private void AccountTypeChanged()
        {
            IsCachAccount = _tradeClient.Account.Type == AccountTypes.Cash;
            IsGrossAccount = _tradeClient.Account.Type == AccountTypes.Gross;
            IsNetAccount = _tradeClient.Account.Type == AccountTypes.Net;

            NotifyOfPropertyChange(nameof(IsCachAccount));
            NotifyOfPropertyChange(nameof(IsGrossAccount));
            NotifyOfPropertyChange(nameof(IsNetAccount));
        }

        private void RemoveFromList(string tradeTransactionId)
        {
            try
            {
                if (_tradesList.ContainsKey(tradeTransactionId))
                    _tradesList.Remove(tradeTransactionId);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private async Task TruncateHistoryAsync(DateTime from, CancellationToken cToken)
        {
            int deletedCount = 0;

            while (_tradesList.Count > 0)
            {
                var firstRecord = _tradesList.GetFirstKeyValue();

                if (firstRecord.Value.CloseTime.ToLocalTime() >= from)
                    break;

                _tradesList.Remove(firstRecord.Key);

                if (deletedCount % 400 == 0)
                    await Dispatcher.Yield(DispatcherPriority.DataBind);

                deletedCount++;
            }
        }

        private async void CleanupLoop()
        {
            while (true)
            {
                if (!_isRefreshing)
                    await AdjustHistoryBoundaries();

                await Task.Delay(CleanUpDelay);
            }
        }
    }
}
