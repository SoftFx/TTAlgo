using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    //public partial class DynamicDataSource<TRow>
    //{
    //    private Dictionary<string, Collection> collections = new Dictionary<string, Collection>();
    //    private List<Reader> readers = new List<Reader>();
    //    private ISyncContext syncObject;

    //    public event Action<Collection> CollectionAdded = delegate { };
    //    public event Action<Collection> CollectionRemoved = delegate { };

    //    internal Collection AddCollectionRef(string id)
    //    {
    //        Collection result;
    //        if (!collections.TryGetValue(id, out result))
    //        {
    //            result = new Collection(id, syncObject);
    //            collections.Add(id, result);
    //            CollectionAdded(result);
    //        }
    //        result.RefCount++;
    //        return result;
    //    }

    //    internal void RemoveCollectionRef(string id)
    //    {
    //        Collection refCollection = collections[id];
    //        refCollection.RefCount--;
    //        if (refCollection.RefCount <= 0)
    //        {
    //            collections.Remove(id);
    //            CollectionRemoved(refCollection);
    //        }
    //    }

    //    public Reader CreateReader(string primaryCollectionId)
    //    {
    //        return syncObject.DoSynchronized(() =>
    //        {
    //            var reader = new Reader(this, AddCollectionRef(primaryCollectionId));
    //            readers.Add(reader);
    //            return reader;
    //        });
    //    }

    //    public interface ISyncContext
    //    {
    //        void DoSynchronized(Action action);
    //        T DoSynchronized<T>(Func<T> action);
    //    }
    //}
}
