using Caliburn.Micro;
using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Threading;
using TickTrader.Algo.Domain;

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

        private ObservableCollection<BaseTransactionModel> _tradesList;
        private ObservableTask<int> _downloadObserver;
        private TraderClientModel _tradeClient;
        private DateTime _from;
        private DateTime _to;
        private DateTime? _currenFrom;
        private DateTime? _currenTo;
        private bool _isNewConnection;
        private bool _currentSkipCanceled;
        private bool _currentSkipTriggers;
        private TimePeriod _period;
        private bool _isRefreshing;
        private bool _isConditionsChanged;
        private TradeDirection _tradeDirectionFilter;
        private bool _skipCancel;
        private bool _clearFlag;
        private bool _skipTriggers;
        private CancellationTokenSource _cancelUpdateSrc;
        private ProfileManager _profileManager;
        private ViewModelStorageEntry _viewPropertyStorage;

        public TradeHistoryViewModel(TraderClientModel tradeClient, ConnectionManager cManager, ProfileManager profileManager = null)
        {
            _period = TimePeriod.LastHour;
            TradeDirectionFilter = TradeDirection.All;
            _skipCancel = true;
            _profileManager = profileManager;

            _tradesList = new ObservableCollection<BaseTransactionModel>();
            GridView = new TradeHistoryGridViewModel(_tradesList, profileManager);
            GridView.Filter = new Predicate<object>(FilterTradesList);

            _tradeClient = tradeClient;
            _tradeClient.Account.AccountTypeChanged += AccountTypeChanged;
            _tradeClient.TradeHistory.OnTradeReport += OnTradeReport;
            _tradeClient.TradeHistory.OnTriggerHistoryReport += OnTriggerReport;

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

        public bool SkipTriggers
        {
            get => _skipTriggers;

            set
            {
                if (_skipTriggers == value)
                    return;

                _skipTriggers = value;

                if (_profileManager != null)
                    _viewPropertyStorage.ChangeProperty(nameof(SkipTriggers), value.ToString());

                NotifyOfPropertyChange();
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

            CalculateTimeBoundaries(out DateTime? newFrom, out DateTime? newTo);

            var globalChange = _currentSkipCanceled != SkipCancel || _currentSkipTriggers != SkipTriggers || _isNewConnection;

            if (newFrom == _currenFrom && newTo == _currenTo && !globalChange)
                return;

            _cancelUpdateSrc = new CancellationTokenSource();

            _isRefreshing = true;

            bool truncate = newFrom != null && (newFrom >= _currenFrom || _currenFrom == null) && newTo == _currenTo && !globalChange;

            _currenFrom = newFrom;
            _currenTo = newTo;
            _currentSkipCanceled = SkipCancel;
            _currentSkipTriggers = SkipTriggers;
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

                var downloadTask = Task.Run(() => DownloadingFullHistoryAsync(from.ToUtcTicks(), to.ToUtcTicks(), SkipCancel, SkipTriggers, cToken));
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

        private async Task<int> DownloadingFullHistoryAsync(UtcTicks? from, UtcTicks? to, bool skipCancel, bool skipTriggers, CancellationToken token)
        {
            var ans = 0;

            var tradeStream = _tradeClient.TradeHistory.GetTradeHistory(from, to, skipCancel);
            ans += await DownloadingHistoryRecords(tradeStream, token);

            if (!skipTriggers)
            {
                var triggerStream = _tradeClient.TradeHistory.GetTriggerReportsHistory(from, to, skipCancel);
                ans += await DownloadingHistoryRecords(triggerStream, token);
            }

            return ans;
        }

        private async Task<int> DownloadingHistoryRecords<T>(Channel<T> historyStream, CancellationToken token)
        {
            const int batchSize = 32;

            if (token.IsCancellationRequested)
                return 0;

            var buffer = new BaseTransactionModel[batchSize];

            while (await historyStream.Reader.WaitToReadAsync())
            {
                if (token.IsCancellationRequested)
                {
                    historyStream.Writer.TryComplete();
                    break;
                }

                var cnt = 0;
                while (cnt < batchSize && historyStream.Reader.TryRead(out var report))
                {
                    // create on thread pool thread, add on UI thread
                    buffer[cnt++] = CreateReportModel(report);
                }

                var taskSrc = new TaskCompletionSource<object>();

                OnUIThread(() => AddToList(buffer, cnt, taskSrc));

                await taskSrc.Task;
            }

            return 0;
        }

        private BaseTransactionModel CreateReportModel<T>(T tTransaction)
        {
            SymbolInfo symbolInfo;

            switch (tTransaction)
            {
                case TradeReportInfo tradeInfo:
                    symbolInfo = GetSymbolInfo(tradeInfo);
                    break;

                case TriggerReportInfo triggerInfo:
                    symbolInfo = GetSymbolInfo(triggerInfo);
                    break;

                default:
                    throw new Exception($"Invalid transaction type {nameof(T)}");
            }

            return BaseTransactionModel.Create(_tradeClient.Account.Type.Value, tTransaction, _tradeClient.Account.BalanceDigits, symbolInfo);
        }

        private SymbolInfo GetSymbolInfo(TradeReportInfo transaction)
        {
            if (IsBalanceOperation(transaction))
                return null;

            var symbolModel = _tradeClient.Symbols.GetOrDefault(transaction.Symbol);

            if (symbolModel == null)
                logger.Warn($"Symbol {transaction.Symbol} not found for TradeTransactionID {transaction.Id}.");

            return symbolModel;
        }

        private SymbolInfo GetSymbolInfo(TriggerReportInfo transaction)
        {
            var symbolModel = _tradeClient.Symbols.GetOrDefault(transaction.Symbol);

            if (symbolModel == null)
                logger.Warn($"Symbol {transaction.Symbol} not found for TriggerTransactionID {transaction.Id}.");

            return symbolModel;
        }

        private static bool IsBalanceOperation(TradeReportInfo item)
        {
            return item.ReportType == TradeReportInfo.Types.ReportType.BalanceTransaction;
        }

        public void Close()
        {
            _tradeClient.Account.AccountTypeChanged -= AccountTypeChanged;
            _tradeClient.Connected -= RefreshHistory;
            _tradeClient.TradeHistory.OnTradeReport -= OnTradeReport;
            _tradeClient.TradeHistory.OnTriggerHistoryReport -= OnTriggerReport;
        }

        private void AddToList(BaseTransactionModel tradeTransaction)
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

        private void AddToList(BaseTransactionModel[] transactions, int cnt, TaskCompletionSource<object> completion)
        {
            try
            {
                for (var i = 0; i < cnt; i++)
                {
                    AddToList(transactions[i]);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            completion.TrySetResult(null);
        }

        private bool MatchesCurrentFilter(TradeReportInfo tradeTransaction)
        {
            if (_skipCancel && (tradeTransaction.ReportType == TradeReportInfo.Types.ReportType.OrderCanceled
                || tradeTransaction.ReportType == TradeReportInfo.Types.ReportType.OrderExpired
                || tradeTransaction.ReportType == TradeReportInfo.Types.ReportType.OrderActivated))
                return false;
            return MatchesCurrentBoundaries(tradeTransaction.CloseTime);
        }

        private bool MatchesCurrentBoundaries(Timestamp reportTime)
        {
            var localTime = reportTime.ToDateTime().ToUniversalTime();

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
                var tradeT = (BaseTransactionModel)o;
                return TradeDirectionFilter == TradeDirection.All || TradeDirectionFilter == Convert(tradeT.Side);
            }
            return false;
        }

        private TradeDirection Convert(BaseTransactionModel.TransactionSide side)
        {
            switch (side)
            {
                case BaseTransactionModel.TransactionSide.Buy: return TradeDirection.Buy;
                case BaseTransactionModel.TransactionSide.Sell: return TradeDirection.Sell;
                default: return TradeDirection.All;
            }
        }

        private void OnTradeReport(TradeReportInfo tradeTransaction)
        {
            if (_tradeClient.Account.Type.HasValue && MatchesCurrentFilter(tradeTransaction))
                AddToList(CreateReportModel(tradeTransaction));
        }

        private void OnTriggerReport(TriggerReportInfo triggerTransaction)
        {
            if (_tradeClient.Account.Type.HasValue && MatchesCurrentBoundaries(triggerTransaction.TransactionTime))
                AddToList(CreateReportModel(triggerTransaction));
        }

        private void AccountTypeChanged()
        {
            GridView.AccType.Value = _tradeClient.Account.Type ?? AccountInfo.Types.Type.Gross;
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
            if (!System.Enum.TryParse(periodProp?.State, out _period))
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
