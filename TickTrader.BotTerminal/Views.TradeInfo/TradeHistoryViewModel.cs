using ActorSharp.Lib;
using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
        private const string StorageDateTimeFormat = "dd-MM-yyyy HH:mm:ss";
        private const DateTimeStyles StorageDateTimeStyle = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum TimePeriod { All, LastHour, Today, Yesterday, CurrentMonth, PreviousMonth, LastThreeMonths, LastSixMonths, LastYear, Custom }
        public enum TradeDirection { All = 1, Buy, Sell }

        private ObservableCollection<TransactionReport> _tradesList;
        private ObservableTask<int> _downloadObserver;
        private TraderClientModel _tradeClient;
        private DateTime _from;
        private DateTime _to;
        private DateTime? _currenFrom;
        private DateTime? _currenTo;
        private bool _isNewConnection;
        private bool _currentSkipCanceled;
        private TimePeriod _period;
        private bool _isRefreshing;
        private bool _isConditionsChanged;
        private TradeDirection _tradeDirectionFilter;
        private bool _skipCancel;
        private bool _clearFlag;
        private CancellationTokenSource _cancelUpdateSrc;
        private ProfileManager _profileManager;
        private ViewModelStorageEntry _viewPropertyStorage;

        public TradeHistoryViewModel(TraderClientModel tradeClient, ConnectionManager cManager, ProfileManager profileManager = null)
        {
            _period = TimePeriod.LastHour;
            TradeDirectionFilter = TradeDirection.All;
            _skipCancel = true;
            _profileManager = profileManager;

            _tradesList = new ObservableCollection<TransactionReport>();
            GridView = new TradeHistoryGridViewModel(_tradesList, profileManager);
            GridView.Filter = new Predicate<object>(FilterTradesList);

            _tradeClient = tradeClient;
            _tradeClient.Account.AccountTypeChanged += AccountTypeChanged;
            _tradeClient.TradeHistory.OnTradeReport += OnReport;

            tradeClient.Connected += () =>
            {
                _isNewConnection = true;
                RefreshHistory();
            };

            cManager.LoggedOut += ClearHistory;

            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }

            RefreshHistory();
            CleanupLoop();
        }

        #region Properties

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

                    if (_profileManager != null)
                        _viewPropertyStorage.ChangeProperty(nameof(Period), value.ToString());

                    NotifyOfPropertyChange(nameof(Period));
                    NotifyOfPropertyChange(nameof(CanEditPeriod));
                    RefreshHistory();
                }
            }
        }

        public DateTime From
        {
            get { return DateTime.SpecifyKind(_from, DateTimeKind.Utc); }
            set
            {
                if (_from == value)
                    return;

                _from = value;

                if (_profileManager != null)
                    _viewPropertyStorage.ChangeProperty(nameof(From), value.ToString(StorageDateTimeFormat));

                NotifyOfPropertyChange(nameof(From));
                RefreshHistory();
            }
        }

        public DateTime To
        {
            get { return DateTime.SpecifyKind(_to, DateTimeKind.Utc); }
            set
            {
                if (_to == value)
                    return;

                _to = value;

                if (_profileManager != null)
                    _viewPropertyStorage.ChangeProperty(nameof(To), value.ToString(StorageDateTimeFormat));

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

                if (_profileManager != null)
                    _viewPropertyStorage.ChangeProperty(nameof(SkipCancel), value.ToString());

                NotifyOfPropertyChange(nameof(SkipCancel));
                RefreshHistory();
            }
        }

        public TradeHistoryGridViewModel GridView { get; }

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

        private void ClearHistory()
        {
            if (!_isRefreshing)
                _tradesList.Clear();
            else
                _clearFlag = true;
        }

        private async Task AdjustHistoryBoundaries()
        {
            if (!_tradeClient.IsConnected.Value)
                return;

            DateTime? newFrom, newTo;
            CalculateTimeBoundaries(out newFrom, out newTo);

            var globalChange = _currentSkipCanceled != SkipCancel || _isNewConnection;

            if (newFrom == _currenFrom && newTo == _currenTo && !globalChange)
                return;

            _cancelUpdateSrc = new CancellationTokenSource();

            _isRefreshing = true;

            bool truncate = newFrom != null && (newFrom >= _currenFrom || _currenFrom == null) && newTo == _currenTo && !globalChange;

            _currenFrom = newFrom;
            _currenTo = newTo;
            _currentSkipCanceled = SkipCancel;
            _isNewConnection = false;

            if (truncate)
                await TruncateHistoryAsync(newFrom.Value, _cancelUpdateSrc.Token);
            else
                await DownloadHistory(newFrom, newTo, _cancelUpdateSrc.Token);

            _isRefreshing = false;

            if (_clearFlag)
            {
                _tradesList.Clear();
                _clearFlag = false;
            }
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
            return TransactionReport.Create(_tradeClient.Account.Type.Value, tTransaction, GetSymbolFor(tTransaction));
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

        private void AddToList(TransactionReport tradeTransaction)
        {
            try
            {
                _tradesList.Add(tradeTransaction);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private bool MatchesCurrentFilter(TradeReportEntity tradeTransaction)
        {
            if (_skipCancel && (tradeTransaction.TradeTransactionReportType == TradeExecActions.OrderCanceled
                || tradeTransaction.TradeTransactionReportType == TradeExecActions.OrderExpired
                || tradeTransaction.TradeTransactionReportType == TradeExecActions.OrderActivated))
                return false;
            return MatchesCurrentBoundaries(tradeTransaction.CloseTime);
        }

        private bool MatchesCurrentBoundaries(DateTime reportTime)
        {
            var localTime = reportTime.ToLocalTime();

            return (_currenFrom == null || localTime >= _currenFrom)
                && (_currenTo == null || localTime < _currenTo);
        }

        private void CalculateTimeBoundaries(out DateTime? from, out DateTime? to)
        {
            var now = DateTime.UtcNow; // fix time point

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
                    from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    to = null;
                    break;
                case TimePeriod.PreviousMonth:
                    from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
                    to = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
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
            GridView?.RefreshItems();
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
            if (_tradeClient.Account.Type.HasValue && MatchesCurrentFilter(tradeTransaction))
                AddToList(CreateReportModel(tradeTransaction));
        }

        private void AccountTypeChanged()
        {
            GridView.AccType.Value = _tradeClient.Account.Type ?? AccountTypes.Gross;
        }

        private async Task TruncateHistoryAsync(DateTime from, CancellationToken cToken)
        {
            int count = 0;

            while (_tradesList.Count > 0)
            {
                if (++count % 400 == 0)
                    await Dispatcher.Yield(DispatcherPriority.DataBind);

                var lastIndex = _tradesList.Count - 1;

                if (_tradesList[lastIndex].CloseTime.ToUniversalTime() >= from)
                    break;

                _tradesList.RemoveAt(lastIndex);
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

        private void UpdateProvider()
        {
            _viewPropertyStorage = _profileManager?.CurrentProfile?.GetViewModelStorage(ViewModelStorageKeys.History);

            var skipProp = _viewPropertyStorage.GetProperty(nameof(SkipCancel));
            if (!bool.TryParse(skipProp?.State, out _skipCancel))
                _skipCancel = true;

            var periodProp = _viewPropertyStorage.GetProperty(nameof(Period));
            if (!Enum.TryParse(periodProp?.State, out _period))
                _period = TimePeriod.LastHour;

            var fromProp = _viewPropertyStorage.GetProperty(nameof(From));
            if (!DateTime.TryParseExact(fromProp?.State, StorageDateTimeFormat, CultureInfo.InvariantCulture, StorageDateTimeStyle, out _from))
                _from = DateTime.UtcNow.Date;

            var toProp = _viewPropertyStorage.GetProperty(nameof(To));
            if (!DateTime.TryParseExact(toProp?.State, StorageDateTimeFormat, CultureInfo.InvariantCulture, StorageDateTimeStyle, out _to))
                _to = DateTime.UtcNow.Date.AddDays(1);

            NotifyOfPropertyChange(nameof(SkipCancel));
            NotifyOfPropertyChange(nameof(Period));
            NotifyOfPropertyChange(nameof(CanEditPeriod));
            NotifyOfPropertyChange(nameof(From));
            NotifyOfPropertyChange(nameof(To));
        }
    }
}
