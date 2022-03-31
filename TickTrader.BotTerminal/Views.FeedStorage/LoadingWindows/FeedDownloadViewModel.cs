using Machinarium.Var;
using System;
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


        public FeedDownloadViewModel(ISymbolCatalog catalog, ISymbolData symbol, SymbolManagerViewModel smbManager) : base(catalog, symbol, "Pre-download symbol")
        {
            _smbManager = smbManager;

            DownloadEnabled = (SelectedSymbol.Value?.IsDownloadAvailable ?? false) & IsReadyProgress;

            _varContext.TriggerOnChange(SelectedSymbol.Var, UpdateAvailableRange);
        }


        public void Download()
        {
            ShowProgressUi.Value = true;
            ProgressObserver.Start(DownloadAsync);
        }

        public void Export()
        {
            _smbManager.Export(new FeedCacheKey(SelectedSymbol.Value?.Name, SelectedTimeFrame.Value, SelectedPriceType.Value));
        }


        private async void UpdateAvailableRange(VarChangeEventArgs<ISymbolData> args)
        {
            if (args.New != null)
            {
                var range = await args.New.GetAvailableRange(Feed.Types.Timeframe.M1);
                UpdateAvailableRange(range);
            }
        }

        private async Task DownloadAsync(IActionObserver observer)
        {
            var timeFrame = SelectedTimeFrame.Value;
            var from = DateTime.SpecifyKind(DateRange.From, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(DateRange.To + TimeSpan.FromDays(1), DateTimeKind.Utc);

            observer?.SetMessage("Downloading... \n");

            if (timeFrame.IsTicks())
                await SelectedSymbol.Value.DownloadTicksWithObserver(observer, timeFrame, from, to);
            else
                await SelectedSymbol.Value.DownloadBarWithObserver(observer, timeFrame, SelectedPriceType.Value, from, to);
        }
    }
}
