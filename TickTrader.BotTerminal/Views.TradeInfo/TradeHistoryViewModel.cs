using Caliburn.Micro;
using NLog;
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
using System.Windows.Threading;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class TradeHistoryViewModel : PropertyChangedBase
    {
        private const int CleanUpDelay = 200;


        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum TimePeriod { All, LastHour, Today, Yesterday, CurrentMonth, PreviousMonth, LastThreeMonths, LastSixMonths, LastYear, Custom }
        public enum TradeDirection { All = 1, Buy, Sell }

        private ObservableSrotedList<string, TransactionReport> _tradesList;
        private ObservableTask<int> _downloadObserver;
        private TraderClientModel _tradeClient;
        private DateTime _from;
        private DateTime _to;
        private TimePeriod _period;
        private TradeDirection _tradeDirectionFilter;

        private CancellationTokenSource _cancelLoadSrc, _cancelCleanSrc;
        private ActionBlock<TransactionReport> _uiUpdater;
        private ActionBlock<string> _cleanUpdater;

        public TradeHistoryViewModel(TraderClientModel tradeClient)
        {
            _period = TimePeriod.LastHour;
            UpdateDateTimePeriod();
            TradeDirectionFilter = TradeDirection.All;

            _tradesList = new ObservableSrotedList<string, TransactionReport>();
            TradesList = CollectionViewSource.GetDefaultView(_tradesList);
            TradesList.Filter = new Predicate<object>(FilterTradesList);

            _tradeClient = tradeClient;
            _tradeClient.Account.AccountTypeChanged += AccountTypeChanged;
            _tradeClient.Account.TradeHistory.OnTradeReport += TradeTransactionReport;

            tradeClient.Connected += RefreshHistory;
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
                    UpdateDateTimePeriod();
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
            CleanUpHistory();

            var currentSrc = new CancellationTokenSource();

            try
            {
                if (_cancelLoadSrc != null)
                    _cancelLoadSrc.Cancel();

                _cancelLoadSrc = currentSrc;

                if (_uiUpdater != null)
                {
                    _uiUpdater.Complete();
                    await _uiUpdater.Completion;
                }
            }
            catch (TaskCanceledException) { logger.Debug("Load task canceled"); }
            catch (OperationCanceledException) { logger.Debug("Load operation canceled"); }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to stop background load task!");
            }

            if (currentSrc.IsCancellationRequested)
                return;

            try
            {
                var options = new ExecutionDataflowBlockOptions()
                {
                    TaskScheduler = DataflowHelper.UiDispatcherBacground,
                    MaxDegreeOfParallelism = 5,
                    MaxMessagesPerTask = 10,
                    BoundedCapacity = 2000,
                    CancellationToken = currentSrc.Token
                };

                var currentUpdater = new ActionBlock<TransactionReport>(r => AddToList(r), options);
                currentUpdater.Post(null);

                _uiUpdater = currentUpdater;

                var downloadTask = _tradeClient.Account.TradeHistory.DownloadingHistoryAsync(
                    From.ToUniversalTime(), To.ToUniversalTime(), currentSrc.Token,
                    r => currentUpdater.SendAsync(r, currentSrc.Token).Wait());
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

        public void Close()
        {
            _tradeClient.Account.AccountTypeChanged -= AccountTypeChanged;
            _tradeClient.Connected -= RefreshHistory;
            _tradeClient.Account.TradeHistory.OnTradeReport -= TradeTransactionReport;
        }

        private void AddToList(TransactionReport tradeTransaction)
        {
            try
            {
                if (tradeTransaction == null)
                    _tradesList.Clear();
                else if (tradeTransaction.CloseTime.ToLocalTime().Between(From, To) && !_tradesList.ContainsKey(tradeTransaction.UniqueId))
                    _tradesList.Add(tradeTransaction.UniqueId, tradeTransaction);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void UpdateDateTimePeriod()
        {
            switch (Period)
            {
                case TimePeriod.All:
                    _from = new DateTime(1980, 01, 01);
                    _to = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
                case TimePeriod.LastHour:
                    _from = DateTime.Now.AddHours(-1);
                    _to = DateTime.Now;
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

            if (!CanEditPeriod)
            {
                NotifyOfPropertyChange(nameof(From));
                NotifyOfPropertyChange(nameof(To));
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

        private void TradeTransactionReport(TransactionReport tradeTransaction)
        {
            _uiUpdater.Post(tradeTransaction);
        }

        private void AccountTypeChanged()
        {
            IsCachAccount = _tradeClient.Account.Type == AccountType.Cash;
            IsGrossAccount = _tradeClient.Account.Type == AccountType.Gross;
            IsNetAccount = _tradeClient.Account.Type == AccountType.Net;

            NotifyOfPropertyChange(nameof(IsCachAccount));
            NotifyOfPropertyChange(nameof(IsGrossAccount));
            NotifyOfPropertyChange(nameof(IsNetAccount));
        }

        private async void CleanUpHistory()
        {
            var currentSrc = new CancellationTokenSource();

            try
            {
                if (_cancelCleanSrc != null)
                    _cancelCleanSrc.Cancel();

                _cancelCleanSrc = currentSrc;

                if (_cleanUpdater != null)
                {
                    _cleanUpdater.Complete();
                    await _cleanUpdater.Completion;
                }
            }
            catch (TaskCanceledException) { logger.Debug("Clean task canceled"); }
            catch (OperationCanceledException) { logger.Debug("Clean operation canceled"); }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to stop background clean task!");
            }

            if (currentSrc.IsCancellationRequested)
                return;

            try
            {
                var options = new ExecutionDataflowBlockOptions()
                {
                    TaskScheduler = DataflowHelper.UiDispatcherBacground,
                    MaxDegreeOfParallelism = 5,
                    MaxMessagesPerTask = 10,
                    BoundedCapacity = 2000,
                    CancellationToken = currentSrc.Token
                };

                var currentUpdater = new ActionBlock<string>(id => RemoveFromList(id), options);

                _cleanUpdater = currentUpdater;

                var cleanTask = CleanUpHistoryAsync(currentSrc.Token, r => currentUpdater.SendAsync(r, currentSrc.Token).Wait());

                await cleanTask;
            }
            catch (TaskCanceledException) { logger.Debug("Clean task canceled"); }
            catch (OperationCanceledException) { logger.Debug("Clean operation canceled"); }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to clean trade history!");
            }
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

        public Task CleanUpHistoryAsync(CancellationToken token, Action<string> reportHandler)
        {
            return Task.Run(async () =>
            {
                token.ThrowIfCancellationRequested();

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    UpdateDateTimePeriod();

                    var toDeleteList = new List<string>();
                    await App.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                        toDeleteList = _tradesList.Where(r => r.CloseTime.ToLocalTime() < From).Select(r => r.UniqueId).ToList()),
                        DispatcherPriority.Background);

                    foreach (var reportId in toDeleteList)
                    {
                        reportHandler(reportId);
                    }

                    await Task.Delay(CleanUpDelay);
                }
            }, token);
        }
    }
}
