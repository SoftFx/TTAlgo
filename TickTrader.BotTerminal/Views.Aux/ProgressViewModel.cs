using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    public class ProgressViewModel : EntityBase, IActionObserver
    {
        public ProgressViewModel()
        {
            Message = AddProperty("");
            IsIndeterminate = AddBoolProperty();
            IsError = AddBoolProperty();
            ProgressMin = AddProperty(0D);
            ProgressMax = AddProperty(100D);
            Progress = AddProperty(0D);
        }

        public BoolProperty IsIndeterminate { get; private set; }
        public BoolProperty IsError { get; private set; }
        public Property<double> ProgressMin { get; private set; }
        public Property<double> ProgressMax { get; private set; }
        public Property<double> Progress { get; private set; }
        public Property<string> Message { get; }

        public void StartProgress(double min, double max)
        {
            Execute.OnUIThread(() =>
            {
                if (min >= max)
                    throw new ArgumentException("Max cannot be less or equal to min!");

                ProgressMin.Value = min;
                ProgressMax.Value = max;
                IsIndeterminate.Clear();
            });
        }

        public void Reset()
        {
            ProgressMin.Value = 0;
            ProgressMax.Value = 100;
            IsIndeterminate.Clear();
            Progress.Value = 0;
            Message.Value = "";
            IsError.Clear();
        }

        public void SetProgress(double val)
        {
            Execute.OnUIThread(() =>
            {
                if (IsIndeterminate.Value)
                    throw new InvalidOperationException("You must call StartProgress() method before setting progress value.");

                if (val < ProgressMin.Value || val > ProgressMax.Value)
                    throw new InvalidOperationException();

                Progress.Value = val;
            });
        }

        public void Start()
        {
            Reset();
            IsIndeterminate.Set();
        }

        public void Stop(string errorMsg = null)
        {
            IsIndeterminate.Clear();
            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                IsError.Set();
                Message.Value = errorMsg;
            }
            else
                Progress = ProgressMax;
        }

        public void SetMessage(string message)
        {
            Execute.OnUIThread(() => Message.Value = message);
        }
    }
}
