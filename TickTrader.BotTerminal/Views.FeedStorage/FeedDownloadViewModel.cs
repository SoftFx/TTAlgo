using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class FeedDownloadViewModel : Screen, IWindowModel, IDisposable
    {
        private readonly VarContext varContext = new VarContext();

        private readonly TraderClientModel _client;
        private readonly ISymbolCatalog _catalog;


        public FeedDownloadViewModel(TraderClientModel clientModel, ISymbolCatalog catalog, ISymbolData symbol = null)
        {
            _catalog = catalog;
            _client = clientModel;

            Symbols = new ObservableCollection<ISymbolData>(catalog.AllSymbols);

            DownloadObserver = new ActionViewModel();
            DateRange = new DateRangeSelectionViewModel();

            SelectedTimeFrame = varContext.AddProperty(Feed.Types.Timeframe.M1);
            SelectedPriceType = varContext.AddProperty(Feed.Types.MarketSide.Bid);
            SelectedSymbol = varContext.AddProperty(symbol);
            ShowDownloadUi = varContext.AddBoolProperty();

            IsRangeLoaded = varContext.AddBoolProperty();
            IsPriceTypeActual = !SelectedTimeFrame.Var.IsTicks();
            IsBusy = DownloadObserver.IsRunning;

            DownloadEnabled = (SelectedSymbol.Value?.IsDownloadAvailable ?? true) & IsRangeLoaded.Var & !IsBusy;
            CancelEnabled = !IsBusy | DownloadObserver.CanCancel;

            varContext.TriggerOnChange(SelectedSymbol.Var, a => UpdateAvailableRange(a.New));

            varContext.TriggerOnChange(IsBusy, a => System.Diagnostics.Debug.WriteLine("IsBusy = " + a.New));
            varContext.TriggerOnChange(DownloadObserver.CanCancel, a => System.Diagnostics.Debug.WriteLine("Observer.CanCancel = " + a.New));

            DisplayName = "Pre-download symbol";
        }

        public IEnumerable<Feed.Types.Timeframe> AvailableTimeFrames => TimeFrameModel.AllTimeFrames;
        public IEnumerable<Feed.Types.MarketSide> AvailablePriceTypes => EnumHelper.AllValues<Feed.Types.MarketSide>();
        public ObservableCollection<ISymbolData> Symbols { get; }
        public DateRangeSelectionViewModel DateRange { get; }
        public ActionViewModel DownloadObserver { get; }

        #region Observable Properties

        public BoolProperty ShowDownloadUi { get; }
        public BoolProperty IsRangeLoaded { get; }
        public Property<ISymbolData> SelectedSymbol { get; }
        public BoolVar DownloadEnabled { get; }
        public BoolVar CancelEnabled { get; }
        public BoolVar IsPriceTypeActual { get; }
        public BoolVar IsBusy { get; private set; }
        public Property<Feed.Types.Timeframe> SelectedTimeFrame { get; }
        public Property<Feed.Types.MarketSide> SelectedPriceType { get; }

        #endregion

        public void Cancel()
        {
            if (IsBusy.Value)
                DownloadObserver.Cancel();
            else
                TryCloseAsync();
        }

        public void Dispose()
        {
            varContext.Dispose();
        }

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            Dispose();
            return base.TryCloseAsync(dialogResult);
        }

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(!IsBusy.Value);
        }

        public void Download()
        {
            ShowDownloadUi.Value = true;
            DownloadObserver.Start(DownloadAsync);
        }

        private async void UpdateAvailableRange(ISymbolData smb)
        {
            IsRangeLoaded.Value = false;
            DateRange.Reset();

            if (smb != null)
            {
                var range = await smb.GetAvailableRange(Feed.Types.Timeframe.M1);

                if (SelectedSymbol.Value == smb)
                {
                    DateTime from = DateTime.UtcNow.Date;
                    DateTime to = from;

                    if (range.Item1 != null && range.Item2 != null)
                    {
                        from = range.Item1.Value;
                        to = range.Item2.Value;
                    }

                    DateRange.UpdateBoundaries(from, to);
                    IsRangeLoaded.Value = true;
                }
            }
        }

        private async Task DownloadAsync(IActionObserver observer)
        {
            var symbol = SelectedSymbol.Value.Name;
            var timeFrame = SelectedTimeFrame.Value;
            var priceType = SelectedPriceType.Value;
            var from = DateTime.SpecifyKind(DateRange.From, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(DateRange.To + TimeSpan.FromDays(1), DateTimeKind.Utc);

            observer?.SetMessage("Downloading... \n");

            try
            {
                if (timeFrame.IsTicks())
                    await _catalog.OnlineCollection[symbol].DownloadTicksWithObserver(observer, timeFrame, from, to);
                else
                    await _catalog.OnlineCollection[symbol].DownloadBarWithObserver(observer, timeFrame, priceType, from, to);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
