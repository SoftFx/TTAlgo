using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public class ReaderBase<TRow>
    {
        private Dictionary<string, IMapping> mappingsById = new Dictionary<string, IMapping>();

        protected void AddMapping(string inputId, IMapping mapping)
        {
            this.mappingsById.Add(inputId, mapping);
        }

        protected IMapping GetMappingOrThrow(string inputId)
        {
            IMapping mapping;
            if (!mappingsById.TryGetValue(inputId, out mapping))
                throw new Exception("Input '" + inputId + "' is not mapped.");
            return mapping;
        }

        protected IEnumerable<IMapping> Mappings { get { return mappingsById.Values; } }

        protected interface IMapping
        {
            void AppendNan();
            void Append(TRow rec);
            void Flush();
        }

        protected class Mapping<TIn> : IMapping
        {
            private List<TIn> cacheBuff = new List<TIn>();
            private Func<TRow, TIn> selector;
            private TIn nanValue;
            private InputDataSeries<TIn> inputProxy;

            public Mapping(Func<TRow, TIn> selector, TIn nanValue = default(TIn))
            {
                this.selector = selector;
                this.nanValue = nanValue;
            }

            internal void SetProxy(InputDataSeries<TIn> inputProxy)
            {
                this.inputProxy = inputProxy;
            }

            public void Append(TRow record)
            {
                cacheBuff.Add(selector(record));
            }

            public void AppendNan()
            {
                cacheBuff.Add(nanValue);
            }

            public void Flush()
            {
                inputProxy.Append(cacheBuff);
                cacheBuff.Clear();
            }
        }
    }
}
