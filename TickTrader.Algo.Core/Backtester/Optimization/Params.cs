using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class Params : IEnumerable<KeyValuePair<string, object>>, ICloneable
    {
        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public long Id { get; }

        public double? Result { get; set; }

        public int Count => Parameters.Count;

        public Params()
        {
            Id = -1;
        }

        public Params(long id)
        {
            Id = id;
        }

        public void Add(string paramId, object paramVal)
        {
            Parameters.Add(paramId, paramVal);
        }

        public string this[int index]
        {
            get
            {
                return Parameters.Keys.ToList()[index];
            }
        }

        public void Apply(IPluginSetupTarget target)
        {
            foreach (var pair in Parameters)
                target.SetParameter(pair.Key, pair.Value);
        }

        #region IEnumerable

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return Parameters.Values.GetHashCode();
        }

        public object Clone()
        {
            var copy = new Params(Id);

            foreach (var p in this)
                copy.Add(p.Key, p.Value);
            
            return copy;
        }

        #endregion
    }
}
