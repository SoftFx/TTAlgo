using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public partial class DynamicDataSource<TRow>
    {
        public class Collection
        {
            private object context;
            private List<TRow> innerCollection = new List<TRow>();

            internal Collection(string id, object syncContext)
            {
                this.Id = id;
                this.context = syncContext;
            }

            public string Id { get; private set; }
            public int Count { get; private set; }
            internal int RefCount { get; set; }

            internal event Action<int, TRow> Updated;

            internal TRow this[int index] { get { return innerCollection[index]; } }

            internal IEnumerable<TRow> TakeAt(int index, int pageSize)
            {
                int size = 0;
                while (size < pageSize && index < Count)
                    yield return this[index++];
            }

            public void Append(TRow row)
            {
                lock (context)
                {
                    innerCollection.Add(row);
                    if (Updated != null)
                        Updated(innerCollection.Count - 1, row);
                }
            }

            public void UpdateLast(TRow row)
            {
                lock (context)
                {
                    int index = innerCollection.Count - 1;
                    innerCollection[index] = row;
                    if (Updated != null)
                        Updated(index, row);
                }
            }
        }
    }
}
