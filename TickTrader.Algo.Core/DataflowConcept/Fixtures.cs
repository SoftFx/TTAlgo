using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.DataflowConcept
{
    public interface IRealtimeFeedStream
    {
        event Action Updated;
    }

    public interface IRealtimeFeedProvider<T>
    {
        void UpdateSubscription(string symbolId, int depth);

        //IRealtimeFeedSeries<T> GetRegularSeries(string symbolId);
    }

    internal class SeriesInputAdapter<T>
    {
        //public SeriesInputAdapter(IRealtimeFeedSeries<T> feed, PluginExecutor executor)
        //{
        //}

        //public void MapToInput()
        //{
        //}
    }

    public class BarSeriesInputFixture
    {
    }

    public class TickInputFixture
    {
    }
}
