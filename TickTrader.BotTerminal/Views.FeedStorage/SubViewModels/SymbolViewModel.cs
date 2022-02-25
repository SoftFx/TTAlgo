﻿using Caliburn.Micro;
using Machinarium.Qnil;
using System.Collections.ObjectModel;
using System.Linq;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal.SymbolManager
{
    internal sealed class SymbolViewModel : PropertyChangedBase
    {
        internal const string TypeHeader = nameof(SymbolType);
        internal const string SecurityHeader = nameof(Security);

        private readonly SymbolManagerViewModel _base;


        public ISymbolData Model { get; }

        public string Description => Model.Info.Description;

        public string Security => string.IsNullOrEmpty(Model.Info.Security) ? Name : Model.Info.Security;

        public string Name => Model.Name;

        public bool IsCustom => Model.IsCustom;

        public bool IsOnline => !Model.IsCustom;

        public string SymbolType => IsCustom ? "Custom" : "Online";

        public double DiskSize => Series.Sum(u => u.Size);


        public ObservableCollection<SeriesViewModel> Series { get; }


        public SymbolViewModel(SymbolManagerViewModel @base, ISymbolData model)
        {
            _base = @base;

            Model = model;
            Series = new ObservableCollection<SeriesViewModel>(model.SeriesCollection.Select(u => new SeriesViewModel(u, _base)));

            Model.SeriesAdded += SeriesAddHandler;
            Model.SeriesRemoved += SeriesRemoveHandler;
            Model.SeriesUpdated += SeriesUpdatedHandler;
        }


        private void SeriesAddHandler(IStorageSeries obj)
        {
            Series.Add(new SeriesViewModel(obj, _base));

            NotifyOfPropertyChange(nameof(DiskSize));
        }

        private void SeriesRemoveHandler(IStorageSeries obj)
        {
            Series.Remove(Series.FirstOrDefault(u => u.Info == obj.Key.FullInfo));

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