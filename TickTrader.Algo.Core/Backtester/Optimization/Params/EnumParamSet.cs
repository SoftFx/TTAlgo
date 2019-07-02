using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class EnumParamSet<T> : ParamSeekSet<T>
    {
        private List<T> _selectedValues = new List<T>();

        public override int Size => _selectedValues.Count;

        protected override T GetValue(int valNo)
        {
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
