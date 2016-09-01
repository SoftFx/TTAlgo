using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public sealed class ObservableTask<TResult> : PropertyChangedBase
    {
        public ObservableTask(Task<TResult> task)
        {
            Task = task;
            if (!task.IsCompleted)
            {
                var _ = WatchTaskAsync(task);
            }
        }
        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task;
            }
            catch { }
            NotifyOfPropertiesChange(task);
        }

        private void NotifyOfPropertiesChange(Task task)
        {
            NotifyOfPropertyChange(nameof(Status));
            NotifyOfPropertyChange(nameof(IsCompleted));
            NotifyOfPropertyChange(nameof(IsNotCompleted));
            if (task.IsCanceled)
            {
                NotifyOfPropertyChange(nameof(IsCanceled));
            }
            else if (task.IsFaulted)
            {
                NotifyOfPropertyChange(nameof(IsFaulted));
                NotifyOfPropertyChange(nameof(Exception));
                NotifyOfPropertyChange(nameof(InnerException));
                NotifyOfPropertyChange(nameof(ErrorMessage));
            }
            else
            {
                NotifyOfPropertyChange(nameof(IsSuccessfullyCompleted));
                NotifyOfPropertyChange(nameof(Result));

            }
        }

        public Task<TResult> Task { get; private set; }
        public TResult Result { get { return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult); } }
        public TaskStatus Status { get { return Task.Status; } }
        public bool IsCompleted { get { return Task.IsCompleted; } }
        public bool IsNotCompleted { get { return !Task.IsCompleted; } }
        public bool IsSuccessfullyCompleted { get { return Task.Status == TaskStatus.RanToCompletion; } }
        public bool IsCanceled { get { return Task.IsCanceled; } }
        public bool IsFaulted { get { return Task.IsFaulted; } }
        public AggregateException Exception { get { return Task.Exception; } }
        public Exception InnerException { get { return (Exception == null) ? null : Exception.InnerException; } }
        public string ErrorMessage { get { return (InnerException == null) ? null : InnerException.Message; } }
    }
}
