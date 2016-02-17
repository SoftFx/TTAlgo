using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Model
{
    public class CollectionComposer<T> : ObservableCollection<T>
    {
        public void AddCollection(ObservableCollection<T> childCollection)
        {
            childCollection.CollectionChanged += ChildCollection_CollectionChanged;
        }

        public void RemoveCollection(ObservableCollection<T> childCollection)
        {
        }

        private void ChildCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
            }
            else
                throw new NotImplementedException("Action " + e.Action + " is not supported");
        }
    }
}
