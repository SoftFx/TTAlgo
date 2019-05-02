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
        private ITimeSequenceRef _timeRef;
        private List<BarEntity> _snapshot;
        private Action<object> _sendUpdateAction;
        private BarEntity _currentBar;
        private bool _sendOnUpdate;
        private bool _sendOnClose;
        private DataSeriesTypes _type;
        private string _streamId;

        public ChartDataCollector(TestDataSeriesFlags seriesFlags, DataSeriesTypes type, string seriesName, Action<object> sendUpdateAction, ITimeSequenceRef timeRef)
        {
            _timeRef = timeRef;
            _sendUpdateAction = sendUpdateAction;
            Init(seriesFlags, type, seriesName);
        }

        public int Count => _snapshot.Count;
        public List<BarEntity> Snapshot => _snapshot;

        public void AppendQuote(double price)
        {
            if (_currentBar.Volume < 0)
            {
                _currentBar.Open = price;
                _currentBar.Close = price;
                _currentBar.High = price;
                _currentBar.Low = price;
                _currentBar.Volume = 1;

                if (_sendOnUpdate)
                    SendUpdate(_currentBar.Clone(), SeriesUpdateActions.Append);
            }
            else
            {
                _currentBar.Append(price, 1);

                if (_sendOnUpdate)
                    SendUpdate(_currentBar.Clone(), SeriesUpdateActions.Update);
            }
        }

        public void OnStop()
        {
            if (_currentBar != null && _currentBar.Volume > 0)
            {
                _snapshot?.Add(_currentBar);

                if (_sendOnClose)
                    SendUpdate(_currentBar, SeriesUpdateActions.Append);
            }
        }

        public void Dispose()
        {
            _timeRef.BarOpened -= _timeRef_BarOpened;
            _timeRef.BarClosed -= _timeRef_BarClosed;
        }

        private void Init(TestDataSeriesFlags seriesFlags, DataSeriesTypes dataType, string seriesId)
        {
            _timeRef.BarOpened += _timeRef_BarOpened;
            _timeRef.BarClosed += _timeRef_BarClosed;

            _type = dataType;
            _streamId = seriesId;

            if (seriesFlags.HasFlag(TestDataSeriesFlags.Snapshot))
                _snapshot = new List<BarEntity>();

            if (seriesFlags.HasFlag(TestDataSeriesFlags.Stream))
            {
                if (seriesFlags.HasFlag(TestDataSeriesFlags.Realtime))
                    _sendOnUpdate = true;
                else
                    _sendOnClose = true;
            }
        }

        private void _timeRef_BarOpened(BarEntity bar)
        {
            _currentBar = new BarEntity();
            _currentBar.OpenTime = bar.OpenTime;
            _currentBar.CloseTime = bar.CloseTime;
            _currentBar.Volume = -1;
        }

        private void _timeRef_BarClosed(BarEntity bar)
        {
            _snapshot?.Add(_currentBar);
            if (_sendOnClose)
                SendUpdate(_currentBar, SeriesUpdateActions.Append);
        }

        private void SendUpdate(BarEntity bar, SeriesUpdateActions action)
        {
            //System.Diagnostics.Debug.WriteLine("BAR - " + bar.OpenTime);

            var update = new DataSeriesUpdate<BarEntity>(_type, _streamId, action, bar);
            _sendUpdateAction(update);
        }
    }
}
