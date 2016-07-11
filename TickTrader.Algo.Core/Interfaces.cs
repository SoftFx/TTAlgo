using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public interface IDataSeriesBuffer
    {
        Type SeriesDataType { get; }
    }

    public interface IAlgoDataReader<TRow>
    {
        TRow ReadAt(int index);
        List<TRow> ReadAt(int index, int pageSize);
        void BindInput(string id, object buffer);
        void Reset();
    }

    public interface IObservableDataReader<TRow> : IAlgoDataReader<TRow>
    {
        event Action<int> Updated;
    }

    public interface IMetadataProvider
    {
        IEnumerable<Symbol> Symbols { get; }
    }

    public interface IDataSeriesProvider
    {

    }

    public interface ICustomDataSeriesProvider
    {
    }

    public interface IAccountDataProvider
    {
        IEnumerable<Position> Positions { get; }
        event EntityHandler<Position> PositionsChanged;
        IEnumerable<Order> Orders { get; }
        event EntityHandler<Order> OrdersChanged;
    }

    public interface IPluginSubscriptionHandler
    {
        void Subscribe(string smbCode, int depth);
        void Unsubscribe(string smbCode);
    }

    public enum EntityUpdateActions { Added, Removed, Updated }

    public delegate void EntityHandler<E>(EntityChangeEventArgs<E> args);

    public class EntityChangeEventArgs<E>
    {
        public EntityChangeEventArgs(E entity, EntityUpdateActions action)
        {
            this.Entity = entity;
            this.Action = action;
        }

        public E Entity { get; private set; }
        public EntityUpdateActions Action { get; private set; }
    }
}
