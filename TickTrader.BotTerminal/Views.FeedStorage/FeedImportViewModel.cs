using Caliburn.Micro;
using Google.Protobuf.WellKnownTypes;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal class FeedImportViewModel : Screen, IWindowModel
    {
        private VarContext _context = new VarContext();
        private List<FeedImporter> _importers = new List<FeedImporter>();

        public FeedImportViewModel(ISymbolCatalog catalog, ISymbolData initialSymbol)
        {
            _importers.Add(new CsvFeedImporter());

            Symbols = new ObservableCollection<ISymbolData>(catalog.AllSymbols); //add with custom

            DisplayName = "Import Series";

            SelectedTimeFrame = _context.AddProperty(Feed.Types.Timeframe.M1);
            SelectedPriceType = _context.AddProperty(Feed.Types.MarketSide.Bid);
            SelectedSymbol = _context.AddProperty<ISymbolData>(initialSymbol);
            SelectedImporter = _context.AddProperty<FeedImporter>(_importers[0]);
            ActionRunner = new ActionViewModel();
            IsActionVisible = _context.AddBoolProperty();

            var importerValid = SelectedImporter.Var.Ref(i => i.CanImport.Var);
            var isNotRunning = !ActionRunner.IsRunning;
            CanImport = isNotRunning & SelectedSymbol.Var.IsNotNull() & importerValid;
            CanCancel = isNotRunning | ActionRunner.CanCancel;
            IsInputEnabled = isNotRunning;
            IsPriceActual = !SelectedTimeFrame.Var.IsTicks();

            _context.TriggerOnChange(SelectedSymbol.Var, a => IsActionVisible.Clear());
            _context.TriggerOnChange(SelectedImporter.Var, a => IsActionVisible.Clear());
            _context.TriggerOnChange(SelectedTimeFrame.Var, a => IsActionVisible.Clear());
            _context.TriggerOnChange(SelectedPriceType.Var, a => IsActionVisible.Clear());
        }

        public IEnumerable<FeedImporter> Importers => _importers;
        public Property<FeedImporter> SelectedImporter { get; }
        public ActionViewModel ActionRunner { get; }
        public Property<Feed.Types.Timeframe> SelectedTimeFrame { get; }
        public Property<Feed.Types.MarketSide> SelectedPriceType { get; }
        public Property<ISymbolData> SelectedSymbol { get; }
        public IEnumerable<Feed.Types.Timeframe> AvailableTimeFrames => EnumHelper.AllValues<Feed.Types.Timeframe>();
        public IEnumerable<Feed.Types.MarketSide> AvailablePriceTypes => EnumHelper.AllValues<Feed.Types.MarketSide>();
        public ObservableCollection<ISymbolData> Symbols { get; }
        public BoolVar CanImport { get; }
        public BoolVar CanCancel { get; }
        public BoolVar IsInputEnabled { get; }
        public BoolVar IsPriceActual { get; }
        public BoolProperty IsActionVisible { get; }

        public void Import()
        {
            IsActionVisible.Set();
            ActionRunner.Start((o, c) => DoImport(o, c));
        }

        private void DoImport(IActionObserver observer, CancellationToken cToken)
        {
            var pageSize = 8000;

            var symbol = SelectedSymbol.Value;
            var importer = SelectedImporter.Value;
            var timeFrame = SelectedTimeFrame.Value;
            var priceType = SelectedPriceType.Value;
            long count = 0;

            observer?.SetMessage("Importing... \n");

            if (!timeFrame.IsTicks())
            {
                var vector = new BarVector(timeFrame);

                foreach (var bar in importer.ImportBars())
                {
                    vector.AppendBarPart(bar);
                    count++;

                    if (vector.Count >= pageSize + 1)
                    {
                        var page = vector.RemoveFromStart(pageSize);
                        symbol.WriteSlice(timeFrame, priceType, page.First().OpenTime, page.Last().CloseTime, page);
                        observer.SetMessage(string.Format("Importing...  {0} bars are imported.", count));
                    }
                }

                if (vector.Count > 0)
                {
                    var page = vector.ToArray();
                    symbol.WriteSlice(timeFrame, priceType, page.First().OpenTime, page.Last().CloseTime, page);
                }

                observer.SetMessage(string.Format("Done importing. {0} bars were imported.", count));
            }
            else
            {
                var page = new List<QuoteInfo>();
                QuoteInfo lastTick = null;

                foreach (var tick in importer.ImportQuotes())
                {
                    if (page.Count >= pageSize)
                    {
                        // we cannot put ticks with same time into different chunks
                        if (lastTick.Time != tick.Time)
                        {
                            symbol.WriteSlice(timeFrame, page[0].Timestamp, tick.Timestamp, page.ToArray());
                            observer.SetMessage(string.Format("Importing...  {0} ticks are imported.", count));
                            page.Clear();
                        }
                    }

                    lastTick = tick;
                    page.Add(tick);
                    count++;
                }

                if (page.Count > 0)
                {
                    var toCorrected = page.Last().Time + TimeSpan.FromTicks(1);
                    symbol.WriteSlice(timeFrame, page[0].Timestamp, toCorrected.ToTimestamp(), page.ToArray());
                }

                observer.SetMessage(string.Format("Done importing. {0} ticks were imported.", count));
            }
        }

        public void Cancel()
        {
            if (ActionRunner.IsRunning.Value)
                ActionRunner.Cancel();
            else
                TryCloseAsync();
        }

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(!ActionRunner.IsRunning.Value);
        }

        //public override void CanClose(Action<bool> callback)
        //{
        //    callback(!ActionRunner.IsRunning.Value);
        //}
    }

    internal abstract class FeedImporter : EntityBase
    {
        public FeedImporter(string name)
        {
            Name = name;
            CanImport = AddBoolProperty();
        }

        public string Name { get; }
        public BoolProperty CanImport { get; }
        public abstract IEnumerable<BarData> ImportBars();
        public abstract IEnumerable<QuoteInfo> ImportQuotes();
    }

    internal class CsvFeedImporter : FeedImporter
    {
        public CsvFeedImporter() : base("CSV")
        {
            CanImport.Set();
            FilePath = AddValidable<string>();
            FilePath.MustBeNotEmpty();

            CanImport.Var = FilePath.IsValid();
        }

        public Validable<string> FilePath { get; }

        public override IEnumerable<BarData> ImportBars()
        {
            int lineNo = 1;

            using (var reader = new StreamReader(FilePath.Value))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    var parts = line.Split(',');
                    if (parts.Length != 6)
                        throw new Exception("Invalid record at line " + lineNo);

                    lineNo++;

                    var bar = new BarData
                    {
                        OpenTime = DateTime.Parse(parts[0]).ToTimestamp(),
                        Open = double.Parse(parts[1]),
                        High = double.Parse(parts[2]),
                        Low = double.Parse(parts[3]),
                        Close = double.Parse(parts[4]),
                        RealVolume = double.Parse(parts[5])
                    };

                    yield return bar;
                }
            }
        }

        public override IEnumerable<QuoteInfo> ImportQuotes()
        {
            int lineNo = 1;

            using (var reader = new StreamReader(FilePath.Value))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    var parts = line.Split(',');
                    var maxDepth = (parts.Length - 1) % 4;
                    if (parts.Length < 5 || maxDepth != 0)
                        throw new Exception("Invalid record at line " + lineNo);

                    lineNo++;

                    var asks = maxDepth > 256
                        ? new QuoteBand[maxDepth]
                        : stackalloc QuoteBand[maxDepth];

                    var bids = maxDepth > 256
                        ? new QuoteBand[maxDepth]
                        : stackalloc QuoteBand[maxDepth];

                    var time = DateTime.Parse(parts[0]);

                    var bidDepth = 0;
                    var askDepth = 0;
                    var partsSpan = parts.AsSpan(1);
                    for (var i = 0; i < maxDepth; i++)
                    {
                        // partsSpan[2] and partsSpan[3] should both have empty strings if band is not present
                        // checking partsSpan[3] should eliminate further bound checks
                        if (!string.IsNullOrEmpty(partsSpan[3]))
                            asks[askDepth++] = new QuoteBand(double.Parse(partsSpan[2]), double.Parse(partsSpan[3]));
                        if (!string.IsNullOrEmpty(partsSpan[1]))
                            bids[bidDepth++] = new QuoteBand(double.Parse(partsSpan[0]), double.Parse(partsSpan[1]));
                    }

                    var data = new QuoteData
                    {
                        Time = time.ToTimestamp(),
                        IsBidIndicative = false,
                        IsAskIndicative = false,
                        AskBytes = ByteStringHelper.CopyFromUglyHack(MemoryMarshal.Cast<QuoteBand, byte>(asks.Slice(askDepth))),
                        BidBytes = ByteStringHelper.CopyFromUglyHack(MemoryMarshal.Cast<QuoteBand, byte>(bids.Slice(bidDepth))),
                    };

                    yield return new QuoteInfo("", data);
                }
            }
        }
    }
}