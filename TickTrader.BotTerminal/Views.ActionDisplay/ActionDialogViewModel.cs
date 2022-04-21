using Caliburn.Micro;
using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal sealed class ActionDialogViewModel : Screen, IWindowModel
    {
        private readonly CancellationTokenSource _cancelSrc;
        private bool _isCompleted;

        public bool IsCancellable => _cancelSrc != null;

        public bool IsIndeterminate => Progress == null;

        public ProgressViewModel Progress { get; private set; }


        public ActionDialogViewModel(string actionName, Func<Task> factory)
        {
            DisplayName = actionName;
            Handle(factory);
        }

        public ActionDialogViewModel(string actionName, System.Action action)
        {
            DisplayName = actionName;
            Handle(() => Task.Factory.StartNew(action));
        }

        public ActionDialogViewModel(string actionName, Action<CancellationToken> action)
        {
            DisplayName = actionName;
            _cancelSrc = new CancellationTokenSource();
            Handle(() => Task.Factory.StartNew(() => action(_cancelSrc.Token)));
        }

        public ActionDialogViewModel(string actionName, Action<IActionObserver> action)
        {
            DisplayName = actionName;
            Progress = new ProgressViewModel();
            Handle(() => Task.Factory.StartNew(() => action(Progress)));
        }

        public ActionDialogViewModel(string actionName, Action<IActionObserver, CancellationToken> action)
        {
            DisplayName = actionName;
            _cancelSrc = new CancellationTokenSource();
            Progress = new ProgressViewModel();
            Handle(() => Task.Factory.StartNew(() => action(Progress, _cancelSrc.Token)));
        }

        private async void Handle(Func<Task> workerTaskFactory)
        {
            Progress = new ProgressViewModel();
            Progress.Start();
            try
            {
                var workerTask = workerTaskFactory();
                await workerTask;
            }
            catch (AggregateException ex)
            {
                var fex = ex.FlattenAsPossible();
                if (!(fex is TaskCanceledException))
                    WindowManager.ShowError(fex.Message, this);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex.Message, this);
            }

            _isCompleted = true;

            await TryCloseAsync();
        }

        public void Cancel()
        {
            _cancelSrc?.Cancel();
        }

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            if (_isCompleted)
                return base.CanCloseAsync(cancellationToken);
            else
                return Task.FromResult(false);
        }
    }
}
