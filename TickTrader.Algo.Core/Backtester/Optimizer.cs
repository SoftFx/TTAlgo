using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core
{
    public class Optimizer : CrossDomainObject, IPluginSetupTarget, ITestExecController
    {
        private readonly AlgoPluginRef _ref;
        private readonly OptimizerCore _core;
        private readonly ISynchronizationContext _sync;
        private readonly Dictionary<string, ParamSeekSet> _params = new Dictionary<string, ParamSeekSet>();
        private OptimizationAlgorithm _seekStrategy;
        private EmulatorStates _innerState = EmulatorStates.Stopped;
        private MetricProvider _mSelector = MetricProvider.Default;

        public Optimizer(AlgoPluginRef pluginRef, ISynchronizationContext updatesSync)
        {
            _ref = pluginRef;
            _sync = updatesSync;
            _core = _ref.CreateObject<OptimizerCore>();
            _core.Factory = _ref.CreateExecutorFactory();
            _core.InitBarStrategy(BarPriceType.Bid);
            IsIsolated = pluginRef.IsIsolated;
        }

        public bool IsIsolated { get; }
        public FeedEmulator Feed => _core.Feed;
        public long MaxCasesNo => _seekStrategy.CaseCount;
        public CommonTestSettings CommonSettings { get; } = new CommonTestSettings();
        public EmulatorStates State { get; private set; }
        public int DegreeOfParallelism { get; set; } = 1;
        public MetricProvider MetricSelector
        {
            get => _mSelector;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("MetricSelector cannot be null!");

                _mSelector = value;
                _core.MetricSelector = value;
            }
        }
        public event Action<EmulatorStates> StateChanged;
        public event Action<Exception> ErrorOccurred;
        public event Action<OptCaseReport, long> CaseCompleted;

        public void SetupParamSeek(string paramId, ParamSeekSet seekSet)
        {
            _params[paramId] = seekSet;
        }

        public void SetSeekStrategy(OptimizationAlgorithm strategy)
        {
            strategy.OnInit(_params);
            _seekStrategy = strategy;
            //_core.SetStrategy(strategy);
        }

        public async Task Run(CancellationToken cToken)
        {
            CommonSettings.Validate();

            if (DegreeOfParallelism <= 0)
                throw new Exception("DegreeOfParallelism must be positive integer!");

            lock (_core)
            {
                if (cToken.IsCancellationRequested)
                    return;

                cToken.Register(() =>
                {
                    lock (_core)
                    {
                        _core.CancelOptimization();

                        if (_innerState == EmulatorStates.Running)
                            ChangedState(EmulatorStates.Stopping);
                    }
                });

                ChangedState(EmulatorStates.Running);
            }

            try
            {
                await Task.Factory.StartNew(() => _core.Run(_seekStrategy, CommonSettings, DegreeOfParallelism, OnReport), TaskCreationOptions.LongRunning);
            }
            finally
            {
                lock (_core) ChangedState(EmulatorStates.Stopped);
            }
        }

        private void OnReport(OptCaseReport rep, long casesLeft)
        {
            _sync.Invoke(() =>
            {
                try
                {
                    CaseCompleted?.Invoke(rep, casesLeft);
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(ex);
                }
            });
        }

        public void ChangedState(EmulatorStates newState)
        {
            lock (_core)
            {
                if (_innerState != newState)
                {
                    _innerState = newState;
                    _sync.Send(() =>
                    {
                        State = newState;
                        StateChanged?.Invoke(newState);
                    });
                }
            }
        }

        #region IPluginSetupTarget

        public void SetParameter(string id, object value) => SetupParamSeek(id, new ConstParam(value));
        public T GetFeedStrategy<T>() where T : FeedStrategy => _core.GetFeedStrategy<T>();
        public void MapInput(string inputName, string symbolCode, Mapping mapping) => _core.MapInput(inputName, symbolCode, mapping);

        #endregion IPluginSetupTarget

        #region ITestExecController

        void ITestExecController.Pause()
        {
            throw new NotImplementedException();
        }

        void ITestExecController.Resume()
        {
            throw new NotImplementedException();
        }

        void ITestExecController.SetExecDelay(int delayMs)
        {
            throw new NotImplementedException();
        }

        #endregion

        public class OptimizerCore : CrossDomainObject, IPluginSetupTarget, IBacktesterSettings, IPluginMetadata, IBacktestQueue
        {
            //private UpdateChannel _channel;
            private int _idSeed;
            private CancellationTokenSource _cancelSrc = new CancellationTokenSource();
            private Action<OptCaseReport, long> _repHandler;
            private TransformBlock<ParamsMessage, OptCaseReport> _workerBlock;
            private ActionBlock<OptCaseReport> _controlBlock;
            private Exception _fatalError;

            public void Init(PluignExecutorFactory factory)
            {
                Factory = factory;
            }

            public FeedStrategy FStrategy { get; private set; }
            public FeedEmulator Feed { get; } = new FeedEmulator();
            //public int MaxCaseCount => SeekStrategy?.CaseCount ?? 0;
            //public string MainSymbol { get; private set; }
            //public TimeFrames MainTimeframe { get; private set; }
            public CommonTestSettings CommonSettings { get; private set; }
            public PluignExecutorFactory Factory { get; set; }
            public OptimizationAlgorithm SeekStrategy { get; private set; }
            public MetricProvider MetricSelector { get; set; } = MetricProvider.Default;
            public int EquityHistoryTargetSize { get; set; } = 500;

            //public void SetStrategy(ParamSeekStrategy strategy)
            //{
            //    SeekStrategy = strategy;
            //    SeekStrategy.OnInit(_params);
            //}

            public void CancelOptimization()
            {
                _cancelSrc.Cancel();
            }

            public void Run(OptimizationAlgorithm sStrategy, CommonTestSettings settings, int degreeOfP, Action<OptCaseReport, long> updateHandler)
            {
                _fatalError = null;
                CommonSettings = settings;
                SeekStrategy = sStrategy;
                _repHandler = updateHandler;

                if (SeekStrategy == null)
                    throw new AlgoException("Optimization strategy is not specified!");

                try
                {
                    Feed.InitStorages();

                    CreateWorkderBlock(degreeOfP);
                    CreateControlBlock();

                    _workerBlock.LinkTo(_controlBlock, new DataflowLinkOptions() { PropagateCompletion = true });

                    SeekStrategy.Start(this, degreeOfP);

                    _controlBlock.Completion.Wait();

                    if (_fatalError != null)
                        throw _fatalError;
                }
                finally
                {
                    Feed.DeinitStorages();
                }
            }

            private OptCaseReport Backtest(ParamsMessage caseCfg)
            {
                var emFixture = SetupEmulation();
                caseCfg.Apply(emFixture.Executor);

                Exception execError = null;

                try
                {
                    if (!emFixture.OnStart())
                        execError = new AlgoException("No data for specified period!");

                    using (_cancelSrc.Token.Register(() => emFixture.CancelEmulation()))
                    {
                        emFixture.EmulateExecution(CommonSettings.WarmupSize, CommonSettings.WarmupUnits);
                    }
                }
                catch (Exception ex)
                {
                    execError = ex;
                }
                finally
                {
                    emFixture.OnStop();
                }

                var report = FilleReport(caseCfg, emFixture.Executor.GetBuilder(), emFixture.Collector, execError);

                emFixture.Dispose();
                emFixture.Executor.Dispose();

                return report;
            }

            private void OnCaseTested(OptCaseReport report)
            {
                if (report.ExecError != null)
                {
                    if (IsFatalError(report.ExecError) && _fatalError == null)
                    {
                        _fatalError = report.ExecError;
                        _cancelSrc.Cancel();
                    }
                }

                var casesLeft = SeekStrategy.OnCaseCompleted(report);
                if (casesLeft <= 0)
                    _workerBlock.Complete();

                _repHandler(report, casesLeft);
            }

            private EmulationControlFixture SetupEmulation()
            {
                var feedClone = Feed.Clone();
                var fStrategyClone = FStrategy.Clone();
                var executor = Factory.CreateExecutor();
                executor.Metadata = this;
                executor.OnUpdate = o => { };

                var emFixture = executor.InitEmulation(this, Metadata.AlgoTypes.Robot, feedClone, fStrategyClone);

                executor.InitSlidingBuffering(4000);

                executor.MainSymbolCode = CommonSettings.MainSymbol;
                executor.TimeFrame = CommonSettings.MainTimeframe;
                executor.InstanceId = "Optimizing-" + Interlocked.Increment(ref _idSeed).ToString();
                executor.Permissions = new PluginPermissions() { TradeAllowed = true };

                return emFixture;
            }

            private OptCaseReport FilleReport(ParamsMessage cfg, PluginBuilder builder, BacktesterCollector collector, Exception error)
            {
                var metric = 0d;

                if (builder != null)
                    metric = MetricSelector.GetMetric(builder, collector.Stats);

                var rep = new OptCaseReport(cfg, metric, collector.Stats, error);

                var equitySize = collector.EquityHistorySize;
                var compactTimeFrame = BarExtentions.AdjustTimeframe(CommonSettings.MainTimeframe, equitySize, EquityHistoryTargetSize, out _);
                var equity = collector.LocalGetEquityHistory(compactTimeFrame);
                var margin = collector.LocalGetMarginHistory(compactTimeFrame);

                rep.Equity = equity.ToList();
                rep.Margin = margin.ToList();

                return rep;
            }

            private void CreateWorkderBlock(int degreeOfP)
            {
                var workerOptions = new ExecutionDataflowBlockOptions();
                workerOptions.MaxDegreeOfParallelism = degreeOfP;
                workerOptions.MaxMessagesPerTask = 1;
                workerOptions.CancellationToken = _cancelSrc.Token;

                _workerBlock = new TransformBlock<ParamsMessage, OptCaseReport>((Func<ParamsMessage, OptCaseReport>)Backtest, workerOptions);
            }

            private void CreateControlBlock()
            {
                var controlBlockOptions = new ExecutionDataflowBlockOptions();
                controlBlockOptions.MaxDegreeOfParallelism = 1;
                controlBlockOptions.MaxMessagesPerTask = 1;
                controlBlockOptions.SingleProducerConstrained = true;
                //controlBlockOptions.CancellationToken = _cancelSrc.Token;

                _controlBlock = new ActionBlock<OptCaseReport>((Action<OptCaseReport>)OnCaseTested, controlBlockOptions);
            }

            private bool IsFatalError(Exception ex)
            {
                return !(ex is StopOutException);
            }

            #region Setup

            public BarStrategy InitBarStrategy(BarPriceType mainPirceTipe)
            {
                var barStrategy = new BarStrategy(mainPirceTipe);
                FStrategy = barStrategy;
                return barStrategy;
            }

            public QuoteStrategy InitQuoteStrategy()
            {
                var qStrategy = new QuoteStrategy();
                FStrategy = qStrategy;
                return qStrategy;
            }

            #endregion

            #region IPluginSetupTarget

            void IPluginSetupTarget.SetParameter(string id, object value)
            {
                throw new NotImplementedException();
            }

            public T GetFeedStrategy<T>() where T : FeedStrategy
            {
                return (T)FStrategy;
            }

            public void MapInput(string inputName, string symbolCode, Mapping mapping)
            {
                // hook to appear in plugin domain
                mapping?.MapInput(this, inputName, symbolCode);
            }

            #endregion

            #region IBacktesterSettings

            JournalOptions IBacktesterSettings.JournalFlags => JournalOptions.Disabled;
            Dictionary<string, TestDataSeriesFlags> IBacktesterSettings.SymbolDataConfig => new Dictionary<string, TestDataSeriesFlags>();
            TestDataSeriesFlags IBacktesterSettings.MarginDataMode => TestDataSeriesFlags.Snapshot;
            TestDataSeriesFlags IBacktesterSettings.EquityDataMode => TestDataSeriesFlags.Snapshot;
            TestDataSeriesFlags IBacktesterSettings.OutputDataMode => TestDataSeriesFlags.Disabled;
            bool IBacktesterSettings.StreamExecReports => false;

            #endregion

            #region IPluginMetadata

            IEnumerable<SymbolEntity> IPluginMetadata.GetSymbolMetadata() => CommonSettings.Symbols.Values;
            IEnumerable<CurrencyEntity> IPluginMetadata.GetCurrencyMetadata() => CommonSettings.Currencies.Values;

            void IBacktestQueue.Enqueue(ParamsMessage caseCfg)
            {
                _workerBlock.Post(caseCfg);
            }

            #endregion
        }
    }
}
