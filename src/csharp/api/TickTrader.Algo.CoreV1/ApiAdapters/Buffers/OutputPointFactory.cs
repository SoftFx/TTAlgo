using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public static class OutputPointFactory
    {
        private static Dictionary<System.Type, object> _knownFactories = new Dictionary<System.Type, object>();

        static OutputPointFactory()
        {
            Add(d => Any.Pack(new DoubleValue { Value = d }), a => a.Unpack<DoubleValue>().Value);
            Add<Api.Marker>(m => Any.Pack(((MarkerEntity)m).GetInfo()), a => MarkerEntity.From(a.Unpack<MarkerInfo>()));
        }

        private static void Add<T>(Func<T, Any> packFunc, Func<Any, T> unpackFunc)
        {
            _knownFactories.Add(typeof(T), new OutputPointFactory<T>(packFunc, unpackFunc));
        }

        public static OutputPointFactory<T> Get<T>()
        {
            if (_knownFactories.TryGetValue(typeof(T), out var f))
                return (OutputPointFactory<T>)f;

            throw new ArgumentException($"{typeof(T).FullName} is not supported");
        }
    }

    public class OutputPointFactory<T>
    {
        private Func<T, Any> _packFunc;
        private Func<Any, T> _unpackFunc;


        public OutputPointFactory(Func<T, Any> packFunc, Func<Any, T> unpackFunc)
        {
            _packFunc = packFunc;
            _unpackFunc = unpackFunc;
        }


        public Any PackValue(T val)
        {
            return _packFunc(val);
        }

        public T UnpackValue(Any val)
        {
            return _unpackFunc(val);
        }
    }
}
