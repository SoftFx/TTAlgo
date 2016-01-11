using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public partial class DynamicDataSource<TRow>
    {
        public class Reader : ReaderBase<TRow>, IObservableDataReader<TRow>, IDisposable
        {
            private DynamicDataSource<TRow> dataSrc;
            private Collection primaryCollection;

            internal Reader(DynamicDataSource<TRow> ds, Collection primaryCollection)
            {
                this.dataSrc = ds;
                this.primaryCollection = primaryCollection;
                this.primaryCollection.Updated += PrimaryCollection_Updated;
            }

            public TRow ReadAt(int index)
            {
                throw new NotImplementedException();
            }

            public List<TRow> ReadAt(int index, int pageSize)
            {
                lock (dataSrc.syncObject)
                {
                    var page = primaryCollection.TakeAt(index, 100).ToList();

                    foreach (TRow row in page)
                    {
                        foreach (var mapping in Mappings)
                            mapping.Append(row);
                    }

                    foreach (var mapping in Mappings)
                        mapping.Flush();

                    return page;
                }
            }

            public void BindInput<T>(string id, InputDataSeries<T> buffer)
            {
                ((Mapping<T>)GetMappingOrThrow(id)).SetProxy(buffer);
            }

            private void PrimaryCollection_Updated(int index, TRow row)
            {
                if (this.Updated != null)
                    Updated(index);
            }

            public event Action<int> Updated;

            public void Dispose()
            {
                lock (dataSrc.syncObject)
                {
                    primaryCollection.Updated -= PrimaryCollection_Updated;
                    dataSrc.RemoveCollectionRef(primaryCollection.Id);
                }
            }
        }
    }
}
