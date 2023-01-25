using LiveChartsCore.Defaults;
using Machinarium.ObservableCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Controls.Chart;

namespace TickTrader.BotTerminal
{
    public sealed class ObservableBarVector : ObservableRangeCollection<FinancialPoint>
    {
        private const int DefaultBarVectorSize = 512;

        private Feed.Types.Timeframe _timeFrame;
        private BarSampler _sampler;
        private int _maxVectorSize;


        internal string Symbol { get; private set; }

        internal Feed.Types.Timeframe Timeframe
        {
            get => _timeFrame;

            private set
            {
                if (_timeFrame == value && _sampler != null)
                    return;

                _timeFrame = value;
                _sampler = BarSampler.Get(value);
            }
        }


        internal event Action VectorInitEvent;
        internal event Action NewBarEvent;
        internal event Action<double?, double?> ApplyNewTickEvent;


        internal ObservableBarVector(string symbol = null, int? size = null)
        {
            Symbol = symbol;
            _maxVectorSize = size ?? DefaultBarVectorSize;
        }


        public void InitNewVector(Feed.Types.Timeframe timeFrame, IEnumerable<BarData> vector, string symbol = null, int? size = null)
        {
            Timeframe = timeFrame;
            _maxVectorSize = size ?? DefaultBarVectorSize;
            Symbol = symbol ?? Symbol;

            Clear();
            AddRange(vector.Select(b => b.ToPoint()));

            VectorInitEvent?.Invoke();
        }


        public void AppendBar(BarData bar) => AppendBarIntenral(bar, false);

        public bool TryAppendBar(BarData bar) => AppendBarIntenral(bar, true);

        private bool AppendBarIntenral(BarData bar, bool noThrow)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);
            var currentBar = Items?.LastOrDefault();

            if (currentBar != null && currentBar.Date.Ticks >= boundaries.Open.Value)
                return noThrow ? false : throw new ArgumentException("Invalid time sequnce!");

            if (boundaries.Open != bar.OpenTime || boundaries.Close != bar.CloseTime)
                return noThrow ? false : throw new ArgumentException("Bar has invalid time boundaries!");

            Add(bar.ToPoint());

            if (Count > _maxVectorSize)
                RemoveAt(0);

            NewBarEvent?.Invoke();

            return true;
        }


        public void ApplyTickUpdate(double? bid, double? ask) => ApplyNewTickEvent?.Invoke(bid, ask);

        public void ApplyBarUpdate(BarData bar)
        {
            AppendBarUpdateInternal(bar, true);
        }

        private bool AppendBarUpdateInternal(BarData bar, bool noThrow)
        {
            var currentBar = Items?.LastOrDefault();

            if (currentBar != null)
            {
                if (bar.OpenTime.Value < currentBar.Date.Ticks)
                    return noThrow ? false : throw new ArgumentException("Invalid time sequnce!");

                if (bar.OpenTime.Value == currentBar.Date.Ticks)
                {
                    currentBar.ApplyBarUpdate(bar);
                    return true;
                }
            }

            return AppendBarIntenral(bar, noThrow);
        }


        internal void ResetVisuals()
        {
            // LiveCharts has some issues with cleaning up visuals

            var pointsCopy = Items.Select(p => new FinancialPoint(p.Date, p.High, p.Open, p.Close, p.Low)).ToArray();
            Clear();
            AddRange(pointsCopy);
        }
    }
}
