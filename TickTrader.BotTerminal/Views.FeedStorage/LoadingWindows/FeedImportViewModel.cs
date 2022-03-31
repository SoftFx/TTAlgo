using Machinarium.Var;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class FeedImportViewModel : BaseSymbolsLoadingWindow
    {
        public BoolVar ImportEnabled { get; }


        public FeedImportViewModel(ISymbolCatalog catalog, ISymbolData symbol) : base(catalog, symbol, "Import Series")
        {
            _isRangeLoaded.Value = true; //because import logic don't use date range

            IsReadyProgress &= FileManager.Ready;
            ImportEnabled = IsReadyProgress;
        }


        public void Import()
        {
            ShowProgressUi.Value = true;
            ProgressObserver.Start(ImportAsync);
        }

        private async Task ImportAsync(IActionObserver observer)
        {
            var timeFrame = SelectedTimeFrame.Value;

            observer?.SetMessage("Importing... \n");

            if (timeFrame.IsTicks())
                await SelectedSymbol.Value.ImportTicksWithObserver(observer, FileManager, timeFrame);
            else
                await SelectedSymbol.Value.ImportBarWithObserver(observer, FileManager, timeFrame, SelectedPriceType.Value);
        }
    }
}