using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using CoreMath = TickTrader.Algo.Core.Math;

namespace TickTrader.BotTerminal
{
    internal class FeedImportViewModel : Screen, IWindowModel
    {
        private VarContext _context = new VarContext();
        private List<FeedImporter> _importers = new List<FeedImporter>();

        public FeedImportViewModel(SymbolCatalog catalog, SymbolData initialSymbol)
        {
            _importers.Add(new CsvFeedImporter());

            Symbols = catalog.ObservableSymbols;

            DisplayName = "Import Series";

            SelectedTimeFrame = _context.AddProperty(TimeFrames.M1);
            SelectedPriceType = _context.AddProperty(BarPriceType.Bid);
            SelectedSymbol = _context.AddProperty<SymbolData>(initialSymbol);
            SelectedImporter = _context.AddProperty<FeedImporter>(_importers[0]);
            ActionRunner = new ActionViewModel();
            IsActionVisible = _context.AddBoolProperty();

            var importerValid = SelectedImporter.Var.Ref(i => i.CanImport.Var);
            var isNotRunning = !ActionRunner.IsRunning;
            CanImport = isNotRunning  & SelectedSymbol.Var.IsNotNull() & importerValid;
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
        public Property<TimeFrames> SelectedTimeFrame { get; }
        public Property<BarPriceType> SelectedPriceType { get; }
        public Property<SymbolData> SelectedSymbol { get; }
        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IEnumerable<BarPriceType> AvailablePriceTypes => EnumHelper.AllValues<BarPriceType>();
        public IObservableList<SymbolData> Symbols { get; }
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
                var vector = new CoreMath.BarVector(timeFrame);

                foreach (var bar in importer.ImportBars())
                {
                    vector.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
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
                var tickVector = new List<QuoteEntity>();

                foreach (var tick in importer.ImportQuotes())
                {
                    tickVector.Add(tick);
                    count++;

                    if (tickVector.Count >= pageSize + 1)
                    {
                        var page = RemoveFromStart(tickVector, pageSize);
                        symbol.WriteSlice(timeFrame, page.First().Time, tickVector.Last().Time, page);
                        observer.SetMessage(string.Format("Importing...  {0} ticks are imported.", count));
                    }
                }

                if (tickVector.Count > 0)
                {
                    var page = tickVector.ToArray();
                    var toCorrected = page.Last().Time + TimeSpan.FromTicks(1);
                    symbol.WriteSlice(timeFrame, page.First().Time, toCorrected, page);
                }

                observer.SetMessage(string.Format("Done importing. {0} ticks were imported.", count));
            }
        }

        private static T[] RemoveFromStart<T>(List<T> list, int pageSize)
        {
            var removedPart = new T[pageSize];
            list.CopyTo(0, removedPart, 0, pageSize);
            list.RemoveRange(0, pageSize);
            return removedPart;
        }

        public void Cancel()
        {
            if (ActionRunner.IsRunning.Value)
                ActionRunner.Cancel();
            else
                TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(!ActionRunner.IsRunning.Value);
        }
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
        public abstract IEnumerable<BarEntity> ImportBars();
        public abstract IEnumerable<QuoteEntity> ImportQuotes();
    }

    internal class CsvFeedImporter : FeedImporter
    {
        public CsvFeedImporter() : base("CSV")
        {
            CanImport.Set();
            FilePath = AddValidable<string>();
            FilePath.MustBeNotEmpy();

            CanImport.Var = FilePath.IsValid();
        }

        public Validable<string> FilePath { get; }

        public override IEnumerable<BarEntity> ImportBars()
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

                    var bar = new BarEntity();
                    bar.OpenTime = DateTime.Parse(parts[0]);
                    bar.Open = double.Parse(parts[1]);
                    bar.High = double.Parse(parts[2]);
                    bar.Low = double.Parse(parts[3]);
                    bar.Close = double.Parse(parts[4]);
                    bar.Volume = double.Parse(parts[5]);

                    yield return bar;
                }
            }
        }

        public override IEnumerable<QuoteEntity> ImportQuotes()
        {
            int lineNo = 1;

            var asks = new List<BookEntry>();
            var bids = new List<BookEntry>();

            using (var reader = new StreamReader(FilePath.Value))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    var parts = line.Split(',');
                    if (parts.Length < 5 || (parts.Length - 1) % 4 != 0)
                        throw new Exception("Invalid record at line " + lineNo);

                    lineNo++;

                    var time = DateTime.Parse(parts[0]);

                    for (int i = 1; i < parts.Length; i += 4)
                    {
                        if (!string.IsNullOrEmpty(parts[i + 0]))
                            bids.Add(new BookEntryEntity(double.Parse(parts[i + 0]), double.Parse(parts[i + 1])));

                        if (!string.IsNullOrEmpty(parts[i + 2]))
                            asks.Add(new BookEntryEntity(double.Parse(parts[i + 2]), double.Parse(parts[i + 3])));
                    }

                    yield return new QuoteEntity("", time, bids.ToArray(), asks.ToArray());

                    asks.Clear();
                    bids.Clear();
                }
            }
        }
    }
}
