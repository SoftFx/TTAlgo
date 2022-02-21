using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Threading;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    public class ProgressViewModel : EntityBase, IActionObserver
    {
        public BoolProperty IsIndeterminate { get; }

        public BoolProperty IsError { get; }

        public Property<double> ProgressMin { get; }

        public Property<double> ProgressMax { get; }

        public Property<double> Progress { get; }

        public Property<string> Message { get; }


        public bool ShowMessages { get; set; } = true;

        public CancellationToken CancelationToken { get; set; }


        public ProgressViewModel()
        {
            Message = AddProperty(string.Empty);
            IsIndeterminate = AddBoolProperty();
            IsError = AddBoolProperty();
            ProgressMin = AddProperty(0D, notifyName: "ProgressMin");
            ProgressMax = AddProperty(100D, notifyName: "ProgressMax");
            Progress = AddProperty(0D, notifyName: "Progress");
        }


        public void StartProgress(double min = 0D, double max = 100D)
        {
            Execute.OnUIThread(() =>
            {
                if (min > max)
                    throw new ArgumentException("Max cannot be less or equal to min!");

                ProgressMin.Value = min;
                ProgressMax.Value = max;
                Progress.Value = min;
                IsIndeterminate.Clear();
            });
        }

        public void Reset()
        {
            Progress.Value = 0;
            ProgressMin.Value = 0;
            ProgressMax.Value = 100;

            Message.Value = string.Empty;
            IsError.Clear();
            IsIndeterminate.Clear();
        }

        public void SetProgress(double val)
        {
            Execute.OnUIThread(() =>
            {
                if (IsIndeterminate.Value)
                    throw new InvalidOperationException("You must call StartProgress() method before setting progress value.");

                Progress.Value = Math.Min(val, ProgressMax.Value);
                Progress.Value = Math.Max(Progress.Value, ProgressMin.Value);
            });
        }

        public void Start()
        {
            Reset();
            IsIndeterminate.Set();
        }

        public void StopProgress(string errorMsg = null)
        {
            IsIndeterminate.Clear();

            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                IsError.Set();
                Message.Value = errorMsg;
            }
        }

        public void SetMessage(string message)
        {
            if (ShowMessages)
                Execute.OnUIThread(() => Message.Value = message);
        }
    }
}
