using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public interface IDataSeriesUpdate
    {
        DataSeriesTypes SeriesType { get; }
        string SeriesId { get; }
    }

    [Serializable]
    public class DataSeriesUpdate<T> : IDataSeriesUpdate
    {
        public DataSeriesUpdate(DataSeriesTypes seriesType, string seriesId, SeriesUpdateActions action, T data)
        {
            SeriesType = seriesType;
            SeriesId = seriesId;
            Action = action;
            Value = data;
        }

        public DataSeriesTypes SeriesType { get; }
        // Symbol name for SymbolRate type. OutputId for Output type. Stream name for NamedStream type (Equity, Margin)
        public string SeriesId { get; }
        public SeriesUpdateActions Action { get; }
        public T Value { get; }
    }

    public enum DataSeriesTypes
    {
        SymbolRate,
        NamedStream,
        Output
    }

    public enum SeriesUpdateActions
    {
        Append,
        Update
    }
}
