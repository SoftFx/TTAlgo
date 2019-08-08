using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TickTrader.BotAgent.Configurator
{
    public class ModelErrorCounter : INotifyPropertyChanged
    {
        private SortedSet<string> _damagedFields { get; }

        public static int TotalErrorCount { get; private set; }

        public int ModelErrorCount => _damagedFields?.Count ?? 0;

        public ModelErrorCounter()
        {
            _damagedFields = new SortedSet<string>();
        }

        public void AddError(string field)
        {
            if (_damagedFields.Contains(field))
                return;

            TotalErrorCount++;
            _damagedFields.Add(field);

            OnPropertyChanged(nameof(ModelErrorCount));
            NotifyStaticPropertyChanged(nameof(TotalErrorCount));
        }

        public void DeleteError(string field)
        {
            if (!_damagedFields.Contains(field))
                return;

            TotalErrorCount--;
            _damagedFields.Remove(field);

            OnPropertyChanged(nameof(ModelErrorCount));
            NotifyStaticPropertyChanged(nameof(TotalErrorCount));
        }

        public void ResetErrors()
        {
            TotalErrorCount -= _damagedFields.Count;
            _damagedFields.Clear();

            OnPropertyChanged(nameof(ModelErrorCount));
            NotifyStaticPropertyChanged(nameof(TotalErrorCount));
        }

        public void CheckStringLength(string str, int minLenght, string field)
        {
            if (str.Length < minLenght)
            {
                AddError(field);
                throw new ArgumentException($"String length less than {minLenght}");
            }
            else
            if (_damagedFields.Contains(field))
                DeleteError(field);
        }

        public void CheckNumberRange(int number, string field, int min = 0, int max = int.MaxValue)
        {
            if (number < min || number > max)
            {
                AddError(field);
                throw new Exception($"Number must be between {min} and {max}");
            }
            else
            if (_damagedFields.Contains(field))
                DeleteError(field);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private static void NotifyStaticPropertyChanged([CallerMemberName] string name = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }
    }
}
