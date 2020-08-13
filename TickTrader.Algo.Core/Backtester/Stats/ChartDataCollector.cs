using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class ChartDataCollector
    {
        private ITimeSequenceRef _timeRef;
        private List<BarData> _snapshot;
        private Action<object> _sendUpdateAction;
        private BarData _currentBar;
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
        public List<BarData> Snapshot => _snapshot;

        public void AppendQuote(double price)
        {
            if (_currentBar == null)
                return;

            if (_currentBar.TickVolume == 0)
            {
                _currentBar.Init(price, 1);

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
            //if (_currentBar != null && _currentBar.Volume > 0)
            //{
            //    _snapshot?.Add(_currentBar);
            //    System.Diagnostics.Debug.WriteLine("EM DATA UPDATE - " + _currentBar.OpenTime);

            //    if (_sendOnClose)
            //        SendUpdate(_currentBar, SeriesUpdateActions.Append);
            //}

            _timeRef.BarOpened -= _timeRef_BarOpened;
            _timeRef.BarClosed -= _timeRef_BarClosed;
        }

        private void Init(TestDataSeriesFlags seriesFlags, DataSeriesTypes dataType, string seriesId)
        {
            if (seriesFlags != TestDataSeriesFlags.Disabled)
            {
                _timeRef.BarOpened += _timeRef_BarOpened;
                _timeRef.BarClosed += _timeRef_BarClosed;

                _type = dataType;
                _streamId = seriesId;

                if (seriesFlags.HasFlag(TestDataSeriesFlags.Snapshot))
                    _snapshot = new List<BarData>();

                if (seriesFlags.HasFlag(TestDataSeriesFlags.Stream))
                {
                    if (seriesFlags.HasFlag(TestDataSeriesFlags.Realtime))
                        _sendOnUpdate = true;
                    else
                        _sendOnClose = true;
                }
            }
        }

        private void _timeRef_BarOpened(BarData bar)
        {
            _currentBar = BarData.CreateBlank(bar.OpenTime, bar.CloseTime);
        }

        private void _timeRef_BarClosed(BarData bar)
        {
            if (_currentBar?.TickVolume > 0)
                _snapshot?.Add(_currentBar);
            if (_sendOnClose)
                SendUpdate(_currentBar, SeriesUpdateActions.Append);
        }

        private void SendUpdate(BarData bar, SeriesUpdateActions action)
        {
            var update = new DataSeriesUpdate<BarData>(_type, _streamId, action, bar);
            _sendUpdateAction(update);
        }
    }
}
