using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AlgoContext
    {
        private Dictionary<string, object> parameters = new Dictionary<string, object>();

        public AlgoContext()
        {
        }

        public IAlgoDataReader Reader { get; set; }
        public IAlgoDataWriter Writer { get; set; }

        public void SetParameter(string paramId, object value)
        {
            parameters[paramId] = value;
        }

        internal IDictionary<string, object> Parameters { get { return parameters; } }
    }
}
