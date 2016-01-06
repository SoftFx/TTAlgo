using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    //public partial class DynamicDataSource<TRow>
    //{
    //    public class Reader : ReaderBase<TRow>, IAlgoDataReader<TRow>, IDisposable
    //    {
    //        private DynamicDataSource<TRow> dataSrc;
    //        private int position;
    //        private int validPosition;
    //        private bool isReading;
    //        private Collection primaryCollection;

    //        internal Reader(DynamicDataSource<TRow> ds, Collection primaryCollection)
    //        {
    //            this.dataSrc = ds;
    //            this.primaryCollection = primaryCollection;
    //            this.primaryCollection.Updated += PrimaryCollection_Updated;
    //        }

    //        public void Start()
    //        {
    //            dataSrc.syncObject.DoSynchronized(() =>
    //            {
    //                Awake();   
    //            });
    //        }

    //        private void Awake()
    //        {
    //            if (!isReading)
    //            {
    //                isReading = true;
    //                Task.Factory.StartNew(DoRead);
    //            }
    //        }

    //        private void DoRead()
    //        {
    //            while (true)
    //            {
    //                //dataSrc.syncObject.DoSynchronized();

    //                int toRead = primaryCollection.Count;
    //            }

    //        }

    //        private void PrimaryCollection_Updated(int index, TRow row)
    //        {
    //            isUpdated = true;
    //            Awake();
    //        }

    //        public event Action<TRow> Appended;
    //        public event Action<IEnumerable<TRow>> AppendedRange;

    //        public event Action Initialized;
    //        public event Action<TRow> Updated;

    //        public IDataSeriesBuffer BindInput(string id, InputFactory factory)
    //        {
    //        }

    //        public void Dispose()
    //        {
    //            dataSrc.syncObject.DoSynchronized(() =>
    //            {
    //                primaryCollection.Updated -= PrimaryCollection_Updated;
    //                dataSrc.RemoveCollectionRef(primaryCollection.Id);
    //            });
    //        }

    //    }
    //}
}
