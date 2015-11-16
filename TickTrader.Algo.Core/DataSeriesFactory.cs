using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class DataSeriesFactory : NoTimeoutByRefObject
    {
        public InputDataSeries<T> CreateInputBuffer<T>()
        {
            return new InputDataSeries<T>();
        }

        public InputDataSeries CreateInputBuffer()
        {
            return new InputDataSeries();
        }

        public OutputDataSeries<T> CreateOuputBuffer<T>()
        {
            return new OutputDataSeries<T>();
        }

        public OutputDataSeries CreateOuputBuffer()
        {
            return new OutputDataSeries();
        }
    }
}
