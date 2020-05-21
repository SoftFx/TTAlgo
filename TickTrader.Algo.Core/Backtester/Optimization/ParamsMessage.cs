using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class ParamsMessage : IEnumerable<KeyValuePair<string, object>>
    {
        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public long Id { get; } = -1;

        public ParamsMessage(long id)
        {
            Id = id;
        }

        public void Add(string paramId, object paramVal)
        {
            Parameters.Add(paramId, paramVal);
        }

        public void Apply(IPluginSetupTarget target)
        {
            foreach (var pair in Parameters)
                target.SetParameter(pair.Key, pair.Value);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }
    }
}
