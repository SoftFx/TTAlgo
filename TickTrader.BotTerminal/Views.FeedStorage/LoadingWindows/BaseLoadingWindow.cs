using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal abstract class BaseLoadingWindow : Screen, IWindowModel
    {
        protected readonly VarContext _varContext = new VarContext();
        protected readonly BoolProperty _isRangeLoaded;


        public BoolProperty ShowProgressUi { get; }

        protected virtual BoolVar IsReadyProgress { get; set; }


        public FileViewManager FileManager { get; }

        public ActionViewModel ProgressObserver { get; }

        public DateRangeSelectionViewModel DateRange { get; }


        internal BaseLoadingWindow(string displayName)
        {
            DisplayName = displayName;

            ProgressObserver = new ActionViewModel();
            DateRange = new DateRangeSelectionViewModel();
            FileManager = new FileViewManager(_varContext, DateRange);

            _isRangeLoaded = _varContext.AddBoolProperty();

            ShowProgressUi = _varContext.AddBoolProperty();
            IsReadyProgress = _isRangeLoaded.Var & !ProgressObserver.IsRunning;
        }


        public void Cancel() => ProgressObserver.Cancel();

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(!ProgressObserver.IsRunning.Value);
        }

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            _varContext.Dispose();
            return base.TryCloseAsync(dialogResult);
        }


        protected void UpdateAvailableRange((DateTime?, DateTime?) range)
        {
            _isRangeLoaded.Value = false;
            DateRange.Reset();

            DateTime from = DateTime.UtcNow.Date;
            DateTime to = from;

            if (range.Item1 != null && range.Item2 != null)
            {
                from = range.Item1.Value;
                to = range.Item2.Value;
            }

            DateRange.UpdateBoundaries(from, to);
            _isRangeLoaded.Value = true;
        }
    }
}
