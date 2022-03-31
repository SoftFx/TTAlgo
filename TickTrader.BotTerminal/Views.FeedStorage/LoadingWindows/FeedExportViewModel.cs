using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class FeedExportViewModel : BaseLoadingWindow
    {
        private readonly IStorageSeries _series;


        public FeedExportViewModel(IStorageSeries series) : base($"Export Series: {series.Key.FullInfo})")
        {
            _series = series;

            FileManager.FileName.Value = series.Key.FullInfo;

            UpdateAvailableRange((_series.From, _series.To));
        }


        public void Export()
        {
            Task ExportAsync(IActionObserver observer)
            {
                observer?.SetMessage("Exporting... \n");

                return _series.ExportSeriesWithObserver(observer, FileManager);
            }

            ShowProgressUi.Value = true;
            ProgressObserver.Start(ExportAsync);
        }
    }
}