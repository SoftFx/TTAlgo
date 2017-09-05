using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    public class ProgressViewModel : ObservableObject, IActionObserver
    {
        public ProgressViewModel()
        {
            Messages = new ObservableCollection<string>();
        }

        public void StartProgress(double min, double max)
        {
            Execute.OnUIThread(() =>
            {
                if (min >= max)
                    throw new ArgumentException("Max cannot be less or equal to min!");

                ProgressMin = min;
                ProgressMax = max;
                IsProgressInitialized = true;
                NotifyOfPropertyChange(nameof(ProgressMin));
                NotifyOfPropertyChange(nameof(ProgressMax));
                NotifyOfPropertyChange(nameof(IsProgressInitialized));
            });
        }

        public void Reset()
        {
            Execute.OnUIThread(() =>
            {
                ProgressMin = 0;
                ProgressMax = 0;
                IsProgressInitialized = false;
                Progress = 0;
                NotifyOfPropertyChange(nameof(ProgressMin));
                NotifyOfPropertyChange(nameof(ProgressMax));
                NotifyOfPropertyChange(nameof(Progress));
                NotifyOfPropertyChange(nameof(IsProgressInitialized));

                for (int i = 0; i < Messages.Count; i++)
                    Messages[i] = "";
            });
        }

        public void SetProgress(double val)
        {
            Execute.OnUIThread(() =>
            {
                if (!IsProgressInitialized)
                    throw new InvalidOperationException("You must call StartProgress() method before setting progress value.");

                if (val < ProgressMin || val > ProgressMax)
                    throw new InvalidOperationException();

                Progress = val;
                NotifyOfPropertyChange(nameof(Progress));
            });
        }

        public bool IsProgressInitialized { get; private set; }
        public double ProgressMin { get; private set; }
        public double ProgressMax { get; private set; }
        public double Progress { get; private set; }

        public ObservableCollection<string> Messages { get; }

        public void SetMessage(int slot, string message)
        {
            Execute.OnUIThread(() =>
            {
                EnsureSlot(slot);
                Messages[slot] = message;
            });
        }

        //public void SetKeyValue(int slot, string key, string value)
        //{
        //    Execute.OnUIThread(() =>
        //    {
        //        EnsureSlot(slot);
        //        Messages[slot] = new KeyValueProgressMessage(key, value);
        //    });
        //}

        private void EnsureSlot(int slotNo)
        {
            for (int i = Messages.Count; i <= slotNo; i++)
                Messages.Add("");
        }

        public void ReserveMessageSlots(int slotCount)
        {
            EnsureSlot(slotCount);
        }
    }

    //public interface IProgressDescriptionMessage
    //{
    //}

    //public class SimpleProgressMessage : IProgressDescriptionMessage
    //{
    //    public SimpleProgressMessage(string val = "")
    //    {
    //        Value = val;
    //    }

    //    public string Value { get; }
    //}

    //public class KeyValueProgressMessage : IProgressDescriptionMessage
    //{
    //    public KeyValueProgressMessage(string key, string value)
    //    {
    //        Key = key;
    //        Value = value;
    //    }

    //    public string Key { get; }
    //    public string Value { get; }
    //}
}
