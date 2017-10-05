using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    class ActionViewModel : ViewAware, IDisposable
    {
        private VarContext _context = new VarContext();
        private Property<CancellationTokenSource> _cancelSrc;
        private BoolProperty _isRunning;
        private BoolProperty _canCancel;
        private BoolProperty _isCanceled;

        public ActionViewModel()
        {
            Progress = new ProgressViewModel();
            _cancelSrc = _context.AddProperty<CancellationTokenSource>();
            _isRunning = _context.AddBoolProperty();
            _isCanceled = _context.AddBoolProperty();
            _canCancel = _context.AddBoolProperty();

            _canCancel.Var = _isRunning.Var & _cancelSrc.Var.IsNotNull() & !_isCanceled.Var;
            _context.TriggerOn(_isCanceled.Var, () => _cancelSrc.Value.Cancel());

        }

        public BoolVar IsRunning => _isRunning.Var;
        public BoolVar IsCancelling => _isCanceled.Var;
        public BoolVar CanCancel => _canCancel.Var;
        public ProgressViewModel Progress { get; private set; }

        public Task Start(System.Action action)
        {
            return Handle(Task.Factory.StartNew(action));
        }

        public Task Start(Action<CancellationToken> action)
        {
            _cancelSrc.Value = new CancellationTokenSource();
            return Handle(Task.Factory.StartNew(() => action(_cancelSrc.Value.Token)));
        }

        public Task Start(Action<IActionObserver> action)
        {
            return Handle(Task.Factory.StartNew(() => action(Progress)));
        }

        public Task Start(string actionName, Action<IActionObserver, CancellationToken> action)
        {
            _cancelSrc.Value = new CancellationTokenSource();
            return Handle(Task.Factory.StartNew(() => action(Progress, _cancelSrc.Value.Token)));
        }

        private async Task Handle(Task workerTask)
        {
            Progress.Start();
            _isRunning.Set();

            try
            {
                await workerTask;
                Progress.Stop();
            }
            catch (AggregateException ex)
            {
                var fex = ex.FlattenAsPossible();
                if (!(fex is TaskCanceledException))
                    Progress.Stop(fex.Message);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Progress.Stop(ex.Message);
            }

            _isRunning.Clear();
            _cancelSrc.Value = null;
        }

        public void Cancel()
        {
            _isCanceled.Set();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
