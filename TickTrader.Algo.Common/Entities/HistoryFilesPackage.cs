using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common
{
    public class HistoryFilesPackage
    {
        public DateTime? From { get; }
        public DateTime? To { get; }
        public List<ArraySegment<byte>> Files { get; }
    }
}
