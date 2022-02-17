using Caliburn.Micro;
using Machinarium.Qnil;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class SymbolViewModel : PropertyChangedBase
    {
        private readonly SymbolManagerViewModel _base;

        internal const string TypeHeader = nameof(SymbolType);
        internal const string SecurityHeader = nameof(Security);


        public ObservableCollection<SeriesViewModel> Series { get; }


        public SymbolViewModel(SymbolManagerViewModel @base, ISymbolData model)
        {
            _base = @base;

            Model = model;

            Series = new ObservableCollection<SeriesViewModel>(model.SeriesCollection.Select(u => new SeriesViewModel(u, _base)));
        }

        public ISymbolData Model { get; }
        public string Description => Model.Info.Description;
        public string Security => Model.Info.Security;
        public string Name => Model.Name;
        public bool IsCustom => Model.IsCustom;
        public bool IsOnline => !Model.IsCustom;
        public string SymbolType => IsCustom ? "Custom" : "Online";
        public double? DiskSize { get; private set; }



        public async Task UpdateSize()
        {
            var toUpdate = Series.ToList();

            double totalSize = 0;

            foreach (var s in toUpdate)
            {
                await s.UpdateSize();
                totalSize += (s.Size ?? 0);
            }

            DiskSize = totalSize;
            NotifyOfPropertyChange(nameof(DiskSize));
        }

        public void ResetSize()
        {
            DiskSize = null;
            NotifyOfPropertyChange(nameof(DiskSize));
            //Series.ForEach(s => s.ResetSize());
        }

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
