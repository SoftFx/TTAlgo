using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class IndicatorBuilder
    {
        public IndicatorBuilder(Type indicatorType, IAlgoDataInput src, IAlgoDataOutput dst)
            : this(AlgoMetadata.Get(indicatorType), src, dst)
        {
        }

        internal IndicatorBuilder(AlgoMetadata indicatorDescriptor, IAlgoDataInput src, IAlgoDataOutput dst)
        {
        }

        public static IndicatorBuilder Create<T>(IAlgoDataInput src, IAlgoDataOutput dst)
            where T : Indicator
        {
            return new IndicatorBuilder(typeof(T), src, dst);
        }
    }

    public interface IAlgoDataInput
    {
        DataSeries<T> GetSeries<T>(string name);
        DataSeries GetSeries(string name);
        event Action Appended;
        event Action Updated;
    }

    public interface IAlgoDataOutput
    {
        DataSeries<T> GetSeries<T>(string name);
        DataSeries GetSeries(string name);
        void AppendEmpty();
    }
}
