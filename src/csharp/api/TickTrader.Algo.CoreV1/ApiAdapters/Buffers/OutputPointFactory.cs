using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public delegate OutputPoint OutputPointFactory<T>(long time, T value);

    public static class OutputPointFactory
    {
        private static Dictionary<Type, object> _knownFactories = new Dictionary<Type, object>();

        static OutputPointFactory()
        {
            Add<double>(PackDouble);
            Add<Api.Marker>(PackMarker);
        }

        private static void Add<T>(OutputPointFactory<T> factory)
        {
            _knownFactories.Add(typeof(T), factory);
        }

        public static OutputPointFactory<T> Get<T>()
        {
            if (_knownFactories.TryGetValue(typeof(T), out var f))
                return (OutputPointFactory<T>)f;

            throw new ArgumentException($"{typeof(T).FullName} is not supported");
        }


        private static OutputPoint PackDouble(long time, double value)
        {
            return new OutputPoint(time, value);
        }

        private static OutputPoint PackMarker(long time, Api.Marker marker)
        {
            return new OutputPoint(time, marker.Y, ((MarkerEntity)marker).GetInfo());
        }
    }
}
