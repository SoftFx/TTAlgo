using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class ActionViewModel : ViewAware, IDisposable
    {
        private readonly VarContext _context = new VarContext();
        private readonly Property<CancellationTokenSource> _cancelSrc;
        private readonly BoolProperty _isRunning;
        private readonly BoolProperty _isCanceled;

        public ActionViewModel()
        {
            Progress = new ProgressViewModel();
            _cancelSrc = _context.AddProperty<CancellationTokenSource>();
            _isRunning = _context.AddBoolProperty();
            _isCanceled = _context.AddBoolProperty();

            CanCancel = _isRunning.Var & _cancelSrc.Var.IsNotNull() & !_isCanceled.Var;
            _context.TriggerOn(_isCanceled.Var, () => _cancelSrc.Value.Cancel());
        }

        public BoolVar IsRunning => _isRunning.Var;
        public BoolVar IsCancelling => _isCanceled.Var;
        public BoolVar WasFaultedOrCancelled => Progress.IsError.Var;
        public BoolVar CanCancel { get; }
        public ProgressViewModel Progress { get; }
        public Task Completion { get; private set; }

        public Task Start(System.Action action)
        {
            return Handle(() => Task.Factory.StartNew(action));
        }

        public void Start(Action<CancellationToken> action)
        {
            _cancelSrc.Value = new CancellationTokenSource();
            Completion = Handle(() => Task.Factory.StartNew(() => action(_cancelSrc.Value.Token)));
        }

        public void Start(Action<IActionObserver> action)
        {
            Completion = Handle(() => Task.Factory.StartNew(() => action(Progress)));
        }

        public void Start(Action<IActionObserver, CancellationToken> action)
        {
            _cancelSrc.Value = new CancellationTokenSource();
            Completion = Handle(() => Task.Factory.StartNew(() => action(Progress, _cancelSrc.Value.Token)));
        }

        public void Start(Func<IActionObserver, Task> asyncAction)
        {
            _cancelSrc.Value = new CancellationTokenSource();

            Progress.CancelationToken = _cancelSrc.Value.Token;
            Completion = Handle(() => asyncAction(Progress));
        }

        private async Task Handle(Func<Task> workerTaskFactory)
        {
            Progress.Start();
            _isRunning.Set();

            try
            {
                await workerTaskFactory();
                Progress.StopProgress();
            }
            catch (AggregateException ex)
            {
                var fex = ex.FlattenAsPossible();
                if (!(fex is TaskCanceledException))
                    Progress.StopProgress(fex.Message);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Progress.StopProgress(ex.Message);
            }

            _isRunning.Clear();
            _cancelSrc.Value = null;
            _isCanceled.Clear();
        }

        public void Cancel()
        {
            if (CanCancel.Value)
            {
                _isCanceled.Set();
                Progress.StopProgress("Canceled.");
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
