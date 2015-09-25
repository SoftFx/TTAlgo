using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class DataReaderBase : IAlgoDataInput
    {

    }


    public class SerialDataReader : IAlgoDataInput
    {
        public DataSeries<T> GetSeriesReader<T>(string name, long maxBufferSize)
        {
        }

        public event Action Appended;
        public event Action Updated;
    }

    public class DataMapping
    {
    }

    public class TimeBasedDataReader : IAlgoDataInput
    {
        public Api.DataSeries<T> GetSeriesReader<T>(string name)
        {
            throw new NotImplementedException();
        }

        public event Action Appended;
        public event Action Updated;
    }
}
