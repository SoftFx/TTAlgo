﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class ModelErrorCounter : BaseViewModel, INotifyPropertyChanged
    {
        private readonly string _validationKey = string.Empty;

        private SortedSet<string> _damagedFields, _warningFields;

        public bool HasModelErrorCount => _damagedFields?.Count > 0;

        public bool HasModelWarningCount => _warningFields?.Count > 0;

        public static int TotalErrorCount { get; private set; }

        public static int TotalWarningCount { get; private set; }

        public ModelErrorCounter()
        {
            _damagedFields = new SortedSet<string>();
            _warningFields = new SortedSet<string>();
        }

        public ModelErrorCounter(string key) : this()
        {
            _validationKey = key;
        }

        public void AddError(string field)
        {
            if (_damagedFields.Contains(field))
                return;

            TotalErrorCount++;
            _damagedFields.Add(field);

            OnPropertyChanged(nameof(HasModelErrorCount));
            NotifyStaticPropertyChanged(nameof(TotalErrorCount));
        }

        public void DeleteError(string field)
        {
            if (!_damagedFields.Contains(field))
                return;

            TotalErrorCount--;
            _damagedFields.Remove(field);

            OnPropertyChanged(nameof(HasModelErrorCount));
            NotifyStaticPropertyChanged(nameof(TotalErrorCount));
        }

        public void DropAllErrors()
        {
            TotalErrorCount -= _damagedFields.Count;
            _damagedFields.Clear();

            OnPropertyChanged(nameof(HasModelErrorCount));
            NotifyStaticPropertyChanged(nameof(TotalErrorCount));
        }

        public void AddWarning(string field)
        {
            if (_warningFields.Contains(field))
                return;

            TotalWarningCount++;
            _warningFields.Add(field);

            OnPropertyChanged(nameof(HasModelWarningCount));
            NotifyStaticPropertyChanged(nameof(TotalWarningCount));
        }

        public void DeleteWarning(string field)
        {
            if (!_warningFields.Contains(field))
                return;

            TotalWarningCount--;
            _warningFields.Remove(field);

            OnPropertyChanged(nameof(HasModelWarningCount));
            NotifyStaticPropertyChanged(nameof(TotalWarningCount));
        }

        public void DropAllWarnings()
        {
            TotalWarningCount -= _warningFields.Count;
            _warningFields.Clear();

            OnPropertyChanged(nameof(HasModelWarningCount));
            NotifyStaticPropertyChanged(nameof(TotalWarningCount));
        }

        public void DropAll()
        {
            DropAllErrors();
            DropAllWarnings();
        }

        public void CheckStringLength(string str, int minLenght, string field)
        {
            string key = $"{_validationKey}{field}";

            if (str.Length < minLenght)
            {
                AddError(key);
                throw new ArgumentException($"{Resources.StringLengthLessEx} {minLenght}");
            }
            else
            if (_damagedFields.Contains(key))
                DeleteError(key);
        }

        public void CheckNumberRange(int number, string field, int min = 1, int max = int.MaxValue)
        {
            string key = $"{_validationKey}{field}";

            if (number < min || number > max)
            {
                AddError(key);
                throw new Exception($"{Resources.NumberRangeEx} {min} and {max}");
            }
            else
            if (_damagedFields.Contains(key))
                DeleteError(key);
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        public static void NotifyStaticPropertyChanged([CallerMemberName] string name = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }
    }
}
