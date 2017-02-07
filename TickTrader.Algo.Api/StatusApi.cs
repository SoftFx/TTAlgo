using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface StatusApi
    {
        void Write(string str, params object[] strParams);
        void WriteLine(string str, params object[] strParams);
        void WriteLine();
        void Flush();
        void Clear();
    }
}
