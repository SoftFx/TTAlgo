using Caliburn.Micro;
using System;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class SeriesViewModel : PropertyChangedBase
    {
        private readonly SymbolManagerViewModel _parent;
        private readonly IStorageSeries _series;


        public string Symbol => _series.Key.Symbol;

        public double Size => Math.Round(_series.Size / (1024 * 1024), 2);

        public string Info { get; }

        public string KeyInfo { get; }


        public SeriesViewModel(IStorageSeries series, SymbolManagerViewModel parent)
        {
            _parent = parent;
            _series = series;

            KeyInfo = series.Key.FullInfo;
            Info = $"{series.Key.TimeFrame}";

            if (!series.Key.TimeFrame.IsTicks())
                Info += $"_{series.Key.MarketSide}";

            series.SeriesUpdated += _ => NotifyOfPropertyChange(nameof(Size));
        }


        public void Export() => _parent.Export(_series.Key);

        public async void Remove() => await _series.TryRemove();
    }
}
