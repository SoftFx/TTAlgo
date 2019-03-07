using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class ChartDataCollector : IDisposable
    {
        private BarSequenceBuilder _builder;
        private List<BarEntity> _snapshot = new List<BarEntity>();
        private Action<object> _sendUpdateAction;

        public ChartDataCollector(TestDataSeriesFlags seriesFlags, DataSeriesTypes type, string seriesName, Action<object> sendUpdateAction, TimeFrames timeFrame)
        {
            _builder = BarSequenceBuilder.Create(timeFrame);
            _sendUpdateAction = sendUpdateAction;
            Init(seriesFlags, type, seriesName);
        }

        public ChartDataCollector(TestDataSeriesFlags seriesFlags, DataSeriesTypes type, string seriesName, Action<object> sendUpdateAction, ITimeSequenceRef timeRef)
        {
            _builder = BarSequenceBuilder.Create(timeRef);
            _sendUpdateAction = sendUpdateAction;
            Init(seriesFlags, type, seriesName);
        }

        public int Count => _snapshot.Count;
        public ITimeSequenceRef Ref => _builder;
        public List<BarEntity> Snapshot => _snapshot;

        public void AppendQuote(DateTime time, double price, double volume)
        {
            _builder.AppendQuote(time, price, volume);
        }

        public BarEntity AppendBarPart(DateTime time, double open, double high, double low, double close, double volume)
        {
            return _builder.AppendBarPart(time, open, high, low, close, volume);
        }

        public void OnStop()
        {
            _builder.CloseSequence();
        }

        public void Dispose()
        {
        }

        private void Init(TestDataSeriesFlags seriesFlags, DataSeriesTypes dataType, string seriesId)
        {
            if (seriesFlags.HasFlag(TestDataSeriesFlags.Snapshot))
                _builder.BarOpened += (b) => _snapshot.Add(b);
            if (seriesFlags.HasFlag(TestDataSeriesFlags.Stream))
            {
                if (seriesFlags.HasFlag(TestDataSeriesFlags.Stream))
                {
                    if (seriesFlags.HasFlag(TestDataSeriesFlags.Realtime))
                    {
                        _builder.BarUpdated += (b) => SendUpdate(b, dataType, seriesId, SeriesUpdateActions.Update);
                        _builder.BarOpened += (b) => SendUpdate(b, dataType, seriesId, SeriesUpdateActions.Append);
                    }
                    else
                        _builder.BarClosed += (b) => SendUpdate(b, dataType, seriesId, SeriesUpdateActions.Append);
                }
            }
        }

        private void SendUpdate(BarEntity bar, DataSeriesTypes type, string streamId, SeriesUpdateActions action)
        {
            //System.Diagnostics.Debug.WriteLine("BAR - " + bar.OpenTime);

            var update = new DataSeriesUpdate<BarEntity>(type, streamId, action, bar);
            _sendUpdateAction(update);
        }
    }
}
