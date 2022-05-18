using Caliburn.Micro;
using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class ActionOverlayViewModel : PropertyChangedBase
    {
        private CancellationTokenSource _cancelSrc;
        private TaskCompletionSource<object> _closeTask;

        public ActionOverlayViewModel(Func<IActionObserver, Task> action)
        {
            Completed = Handle(() => action(Progress));
        }

        public ActionOverlayViewModel(Func<IActionObserver, CancellationToken, Task> action)
        {
            _cancelSrc = new CancellationTokenSource();
            Completed = Handle(() => action(Progress, _cancelSrc.Token));
        }

        public ProgressViewModel Progress { get; } = new ProgressViewModel();
        public bool CanCancel => _cancelSrc != null && !_cancelSrc.IsCancellationRequested;
        public bool CanClose { get; private set; }
        public Task Completed { get; }
        public bool ErrorPhase { get; private set; }

        public void Cancel()
        {
            _cancelSrc.Cancel();
            NotifyOfPropertyChange(nameof(CanCancel));
        }

        public void Close()
        {
            _closeTask.SetResult(this);

            CanClose = false;
            NotifyOfPropertyChange(nameof(CanClose));
        }

        private async Task Handle(Func<Task> workerTaskFactory)
        {
            bool noErrors = false;
            Progress.Start();
            try
            {
                await workerTaskFactory();
                Progress.StopProgress();
                noErrors = true;
            }
            catch (AggregateException ex)
            {
                var fex = ex.FlattenAsPossible();
                if (!(fex is TaskCanceledException))
                    Progress.StopProgress(fex.Message);
            }
            catch (TaskCanceledException)
            {
                Progress.StopProgress("Canceled.");
            }
            catch (Exception ex)
            {
                Progress.StopProgress(ex.Message);
            }

            if (noErrors)
                return;

            _closeTask = new TaskCompletionSource<object>();

            ErrorPhase = true;
            CanClose = true;
            NotifyOfPropertyChange(nameof(ErrorPhase));
            NotifyOfPropertyChange(nameof(CanClose));

            await _closeTask.Task;
        }
    }
}
