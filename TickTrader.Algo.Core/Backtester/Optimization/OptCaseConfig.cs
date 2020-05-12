using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OptCaseConfig : IEnumerable<KeyValuePair<string, object>>
    {
        public Dictionary<string, object> Params { get; } = new Dictionary<string, object>();
        public long Id { get; }

        public double Result { get; set; }

        public int Size => Params.Count;

        public OptCaseConfig(long id)
        {
            Id = id;
        }

        public void Add(string paramId, object paramVal)
        {
            Params.Add(paramId, paramVal);
        }

        public void Apply(IPluginSetupTarget target)
        {
            foreach (var pair in Params)
                target.SetParameter(pair.Key, pair.Value);
        }

        #region IEnumerable

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return Params.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Params.GetEnumerator();
        }

        #endregion
    }
}
