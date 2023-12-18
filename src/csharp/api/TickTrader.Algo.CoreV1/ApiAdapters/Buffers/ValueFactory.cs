using System;
using System.Collections.Generic;

namespace TickTrader.Algo.CoreV1
{
    internal sealed class ValueFactory
    {
        private static Dictionary<Type, object> _predefinedValues = new Dictionary<Type, object>();

        static ValueFactory()
        {
            Add(() => float.NaN);
            Add(() => double.NaN);
            Add(() => DateTime.MinValue);
            Add<Api.Bar>(() => BarEntity.Empty);
            Add<Api.Marker>(() => new MarkerEntity());
        }

        private static void Add<T>(Func<T> factoryFunc)
        {
            _predefinedValues.Add(typeof(T), new ValueFactory<T>(factoryFunc));
        }

        public static ValueFactory<T> Get<T>()
        {
            object predefined;
            if (_predefinedValues.TryGetValue(typeof(T), out predefined))
                return (ValueFactory<T>)predefined;
            return new ValueFactory<T>(() => default(T));
        }
    }

    internal class ValueFactory<T>
    {
        private Func<T> _newValueFactory;
        private Func<T> _nullValueFactory;

        public ValueFactory(Func<T> valueFactory)
            : this(valueFactory, valueFactory)
        {
        }

        public ValueFactory(Func<T> newValueFactory, Func<T> nullValueFactory)
        {
            _newValueFactory = newValueFactory;
            _nullValueFactory = nullValueFactory;
        }

        public T GetNullValue() => _nullValueFactory();

        public T GetNewValue() => _newValueFactory();
    }
}
