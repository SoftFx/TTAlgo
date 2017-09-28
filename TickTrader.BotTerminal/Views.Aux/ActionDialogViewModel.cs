using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal class ActionDialogViewModel : Screen, IWindowModel
    {
        private CancellationTokenSource _cancelSrc;
        private bool _isCompleted;

        public ActionDialogViewModel(string actionName, System.Action action)
        {
            DisplayName = actionName;
            Handle(Task.Factory.StartNew(action));
        }

        public ActionDialogViewModel(string actionName, Action<CancellationToken> action)
        {
            DisplayName = actionName;
            _cancelSrc = new CancellationTokenSource();
            Handle(Task.Factory.StartNew(() => action(_cancelSrc.Token)));
        }

        public ActionDialogViewModel(string actionName, Action<IActionObserver> action)
        {
            DisplayName = actionName;
            Progress = new ProgressViewModel();
            Handle(Task.Factory.StartNew(() => action(Progress)));
        }

        public ActionDialogViewModel(string actionName, Action<IActionObserver, CancellationToken> action)
        {
            DisplayName = actionName;
            _cancelSrc = new CancellationTokenSource();
            Progress = new ProgressViewModel();
            Handle(Task.Factory.StartNew(() => action(Progress, _cancelSrc.Token)));
        }

        private async void Handle(Task workerTask)
        {
            try
            {
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
            TryClose();
        }

        public void Cancel()
        {
            _cancelSrc?.Cancel();
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(_isCompleted);
        }

        public bool IsCancellable => _cancelSrc != null;
        public bool IsIndeterminate => Progress == null;
        public ProgressViewModel Progress { get; } 
    }
}
