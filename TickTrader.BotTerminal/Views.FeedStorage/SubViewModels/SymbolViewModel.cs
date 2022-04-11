using Caliburn.Micro;
using Machinarium.Qnil;
using System.Collections.ObjectModel;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class SymbolViewModel : PropertyChangedBase
    {
        internal const string TypeHeader = nameof(SymbolType);
        internal const string SecurityHeader = nameof(Security);

        private readonly SymbolManagerViewModel _base;


        public ISymbolData Model { get; }

        public string Security { get; }

        public string Name => Model.Name;

        public bool IsCustom => Model.IsCustom;

        public bool IsOnline => !Model.IsCustom;

        public string Description => Model.Info.Description;

        public string SymbolType => IsCustom ? "Custom" : "Online";

        public double DiskSize => Series.Sum(u => u.Size);


        public ObservableCollection<SeriesViewModel> Series { get; }


        public SymbolViewModel(SymbolManagerViewModel @base, ISymbolData model)
        {
            _base = @base;

            Model = model;
            Security = GetSecurity(model.Info);
            Series = new ObservableCollection<SeriesViewModel>(model.Series.Select(u => new SeriesViewModel(u.Value, _base)));

            Model.SeriesAdded += SeriesAddHandler;
            Model.SeriesRemoved += SeriesRemoveHandler;
            Model.SeriesUpdated += SeriesUpdatedHandler;
        }


        private static string GetSecurity(ISymbolInfo info)
        {
            if (info.Name.EndsWith("_L"))
                return "Lasts";

            return string.IsNullOrEmpty(info.Security) ? info.Name : info.Security;
        }

        private void SeriesAddHandler(IStorageSeries obj)
        {
            Series.Add(new SeriesViewModel(obj, _base));

            NotifyOfPropertyChange(nameof(DiskSize));
        }

        private void SeriesRemoveHandler(IStorageSeries obj)
        {
            Series.Remove(Series.FirstOrDefault(u => u.KeyInfo == obj.Key.FullInfo));

            NotifyOfPropertyChange(nameof(DiskSize));
        }

        private void SeriesUpdatedHandler(IStorageSeries obj) => NotifyOfPropertyChange(nameof(DiskSize));


        public void Download()
        {
            _base.Download(Model);
        }

        public void Remove()
        {
            _base.RemoveSymbol(Model);
        }

        public void Import()
        {
            _base.Import(Model);
        }

        public void Edit()
        {
            _base.EditSymbol(Model);
        }

        public void Copy()
        {
            _base.CopySymbol(Model);
        }
    }
}
