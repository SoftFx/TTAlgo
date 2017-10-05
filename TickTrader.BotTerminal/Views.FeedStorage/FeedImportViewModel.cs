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
using TickTrader.Algo.Core;
using CoreMath = TickTrader.Algo.Core.Math;

namespace TickTrader.BotTerminal
{
    internal class FeedImportViewModel : Screen, IWindowModel
    {
        private VarContext _context = new VarContext();
        private List<FeedImporter> _importers = new List<FeedImporter>();

        public FeedImportViewModel(IDynamicListSource<ManagedCustomSymbol> symbols, ManagedCustomSymbol initialSymbol)
        {
            _importers.Add(new CsvFeedImporter());

            Symbols = symbols.AsObservable();

            SelectedTimeFrame = _context.AddProperty(TimeFrames.M1);
            SelectedPriceType = _context.AddProperty(BarPriceType.Bid);
            SelectedSymbol = _context.AddProperty<ManagedCustomSymbol>(initialSymbol);
            SelectedImporter = _context.AddProperty<FeedImporter>(_importers[0]);
            ActionRunner = new ActionViewModel();
            IsActionVisible = _context.AddBoolProperty();

            var importerValid = SelectedImporter.Var.Ref(i => i.CanImport.Var);
            var isNotRunning = !ActionRunner.IsRunning;
            CanImport = isNotRunning  & SelectedSymbol.Var.IsNotNull() & importerValid;
            CanCancel = isNotRunning | ActionRunner.CanCancel;
            IsInputEnabled = isNotRunning;
            IsPriceActual = SelectedTimeFrame.Var != TimeFrames.Ticks;

            _context.TriggerOnChange(SelectedSymbol.Var, a => IsActionVisible.Clear());
            _context.TriggerOnChange(SelectedImporter.Var, a => IsActionVisible.Clear());
            _context.TriggerOnChange(SelectedTimeFrame.Var, a => IsActionVisible.Clear());
            _context.TriggerOnChange(SelectedPriceType.Var, a => IsActionVisible.Clear());

            //_context.TriggerOn(importerValid, () => System.Diagnostics.Debug.WriteLine("Importer Valid!"), () => System.Diagnostics.Debug.WriteLine("Importer Invalid!"));
        }

        public IEnumerable<FeedImporter> Importers => _importers;
        public Property<FeedImporter> SelectedImporter { get; }
        public ActionViewModel ActionRunner { get; }
        public Property<TimeFrames> SelectedTimeFrame { get; }
        public Property<BarPriceType> SelectedPriceType { get; }
        public Property<ManagedCustomSymbol> SelectedSymbol { get; }
        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public IEnumerable<BarPriceType> AvailablePriceTypes => EnumHelper.AllValues<BarPriceType>();
        public IObservableListSource<ManagedCustomSymbol> Symbols { get; }
        public BoolVar CanImport { get; }
        public BoolVar CanCancel { get; }
        public BoolVar IsInputEnabled { get; }
        public BoolVar IsPriceActual { get; }
        public BoolProperty IsActionVisible { get; }

        public void Import()
        {
            IsActionVisible.Set();
            ActionRunner.Start(DoImport);
        }

        private void DoImport(CancellationToken cToken)
        {
            var pageSize = 1000;

            var symbol = SelectedSymbol.Value;
            var importer = SelectedImporter.Value;
            var timeFrame = SelectedTimeFrame.Value;
            var priceType = SelectedPriceType.Value;

            if (timeFrame != TimeFrames.Ticks)
            {
                var vector = new CoreMath.BarVector(timeFrame);

                foreach (var bar in importer.ImportBars())
                {
                    vector.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);

                    if (vector.Count >= pageSize + 1)
                    {
                        var page = vector.RemoveFromStart(pageSize);
                        symbol.WriteSlice(timeFrame, priceType, page.First().OpenTime, page.Last().CloseTime, page);
                    }
                }

                if (vector.Count > 0)
                {
                    var page = vector.ToArray();
                    symbol.WriteSlice(timeFrame, priceType, page.First().OpenTime, page.Last().CloseTime, page);
                }
            }
            else
                throw new Exception("Tick import is not supported by now!");

            //Task.Delay(5000).Wait();
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
    }

    internal class CsvFeedImporter : FeedImporter
    {
        public CsvFeedImporter() : base("CSV")
        {
            CanImport.Set();
            FilePath = AddValidable<string>();
            FilePath.MustBeNotEmpy();

            CanImport.Var = FilePath.IsValid();
            //TriggerOn(CanImport.Var, () => System.Diagnostics.Debug.WriteLine("CSV Valid!"), () => System.Diagnostics.Debug.WriteLine("CSV Invalid!"));
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
    }
}
