using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Backtester
{
    public class EnumParamSet<T> : ParamSeekSet<T>
    {
        private List<T> _selectedValues = new List<T>();

        public EnumParamSet()
        {
        }

        public EnumParamSet(IEnumerable<T> selectedValues)
        {
            _selectedValues.AddRange(selectedValues);
        }

        public override int Size => _selectedValues.Count;

        protected override T GetValue(int valNo)
        {
            valNo = Math.Min(valNo, _selectedValues.Count - 1);
            valNo = Math.Max(valNo, 0);
            return _selectedValues[valNo];
        }

        public void Add(T val)
        {
            if (!_selectedValues.Contains(val))
                _selectedValues.Add(val);
        }

        public void Remove(T val)
        {
            _selectedValues.Remove(val);
        }
    }
}
