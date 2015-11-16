using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public interface IAlgoDataReader
    {
        int ReadNext();
        void Update();
        IDataSeriesBuffer GetInputBuffer(string id);
    }

    public interface IAlgoDataWriter
    {
        void Extend();
        IDataSeriesBuffer GetOutputBuffer(string id);
    }
}
