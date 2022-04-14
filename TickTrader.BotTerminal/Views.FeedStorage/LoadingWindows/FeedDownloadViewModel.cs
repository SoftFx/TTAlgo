using Machinarium.Var;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class FeedDownloadViewModel : BaseSymbolsLoadingWindow
    {
        private readonly SymbolManagerViewModel _smbManager;


        public BoolVar DownloadEnabled { get; }

        public BoolProperty ExportEnabled { get; }


        public FeedDownloadViewModel(ISymbolCatalog catalog, ISymbolData symbol, SymbolManagerViewModel smbManager) : base(catalog, symbol, "Pre-download symbol")
        {
            _smbManager = smbManager;

            DownloadEnabled = (SelectedSymbol.Value?.IsDownloadAvailable ?? false) & IsReadyProgress;
            ExportEnabled = _varContext.AddBoolProperty();

            _varContext.TriggerOnChange(SelectedSymbol.Var, UpdateMetadata);
            _varContext.TriggerOnChange(SelectedTimeFrame.Var, UpdateMetadata);
            _varContext.TriggerOnChange(SelectedPriceType.Var, UpdateMetadata);
        }


        public void Download()
        {
            ShowProgressUi.Value = true;
            ExportEnabled.Value = false;
            ProgressObserver.Start(DownloadAsync);
        }

        public void Export()
        {
            if (SelectedSymbol.Value != null)
                _smbManager.Export(new FeedCacheKey(SelectedSymbol.Value.Name, SelectedTimeFrame.Value, SelectedSymbol.Value.Origin, SelectedPriceType.Value));
        }


        private void UpdateMetadata<T>(VarChangeEventArgs<T> _)
        {
            UpdateAvailableRange();
            CheckExistingKey();
        }

        private async void UpdateAvailableRange()
        {
            var symbol = SelectedSymbol.Value;

            if (symbol != null)
            {
                var range = await symbol.GetAvailableRange(SelectedTimeFrame.Value, SelectedPriceType.Value);
                UpdateAvailableRange(range);
            }
        }

        private void CheckExistingKey()
        {
            if (SelectedSymbol.Value == null)
                return;

            var symbol = SelectedSymbol.Value;
            var priceType = SelectedPriceType.Value;
            var timeframe = SelectedTimeFrame.Value;

            var key = new FeedCacheKey(symbol.Name, timeframe, symbol.Origin, IsSelectedTick.Value ? (Feed.Types.MarketSide?)null : priceType);

            ExportEnabled.Value = symbol.Series.TryGetValue(key, out _);
        }

        private async Task DownloadAsync(IActionObserver observer)
        {
            var timeFrame = SelectedTimeFrame.Value;
            var from = DateRange.From;
            var to = DateRange.To;

            observer?.SetMessage("Downloading... \n");

            if (timeFrame.IsTicks())
                await SelectedSymbol.Value.DownloadTicksWithObserver(observer, timeFrame, from, to);
            else
                await SelectedSymbol.Value.DownloadBarWithObserver(observer, timeFrame, SelectedPriceType.Value, from, to);

            CheckExistingKey();
        }
    }
}
