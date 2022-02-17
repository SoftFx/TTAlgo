using System;
using System.Threading.Tasks;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class SeriesViewModel : ObservableObject
    {
        private readonly SymbolManagerViewModel _parent;
        private readonly IStorageSeries _series;


        public string Symbol => _series.Key.Symbol;

        public string Info => _series.Key.FullInfo;


        public double? Size { get; private set; }


        public SeriesViewModel(IStorageSeries series, SymbolManagerViewModel parent)
        {
            _parent = parent;
            _series = series;
        }


        public async Task UpdateSize()
        {
            var newSize = _series.Size;
            Size = Math.Round(newSize.Value / (1024 * 1024), 2);
            NotifyOfPropertyChange(nameof(Size));
        }

        public void ResetSize()
        {
            Size = null;
            NotifyOfPropertyChange(nameof(Size));
        }

        public void Export()
        {
            _parent.Export(this);
        }

        public async void Remove()
        {
            await _series.TryRemove();
        }
    }
}
