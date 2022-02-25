using System;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class SeriesViewModel : ObservableObject
    {
        private readonly SymbolManagerViewModel _parent;
        private readonly IStorageSeries _series;


        public string Symbol => _series.Key.Symbol;

        public string Info => _series.Key.FullInfo;

        public double Size => Math.Round(_series.Size / (1024 * 1024), 2);


        public SeriesViewModel(IStorageSeries series, SymbolManagerViewModel parent)
        {
            _parent = parent;
            _series = series;

            series.SeriesUpdated += _ => NotifyOfPropertyChange(nameof(Size));
        }


        public void Export() => _parent.Export(this);

        public async void Remove() => await _series.TryRemove();
    }
}
