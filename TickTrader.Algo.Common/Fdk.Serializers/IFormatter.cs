using System.Collections.Generic;
using System.IO;

namespace TickTrader.Server.QuoteHistory.Serialization
{
    public interface IFormatter<T>
    {
        string Serialize(T item);
        string Serialize(IEnumerable<T> items);
        T Deserialize(string line);
        void Serialize(StreamWriter stream, IEnumerable<T> items);
    }
}