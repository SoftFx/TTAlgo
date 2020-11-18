using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class AlgoListViewModel : PropertyChangedBase
    {
        private string _filterString;
        private AlgoAgentViewModel _selectedAgent;

        public IObservableList<AlgoAgentViewModel> AvailableAgents { get; }

        public AlgoAgentViewModel SelectedAgent
        {
            get { return _selectedAgent; }
            set
            {
                if (_selectedAgent == value)
                    return;

                _selectedAgent = value;
                NotifyOfPropertyChange(nameof(SelectedAgent));
                PluginViewUpdate();
            }
        }

        public string FilterString
        {
            get => _filterString;
            set
            {
                if (value == _filterString)
                    return;

                _filterString = value;
                NotifyOfPropertyChange(nameof(FilterString));
                PluginsView.Refresh();
            }
        }

        public ICollectionView PluginsView { get; private set; }

        public AlgoListViewModel(AlgoEnvironment algoEnv)
        {
            AvailableAgents = algoEnv.Agents.AsObservable();
            SelectedAgent = AvailableAgents.First();
        }

        private void PluginViewUpdate()
        {
            PluginsView = CollectionViewSource.GetDefaultView(SelectedAgent.Plugins?.AsObservable());
            PluginsView.SortDescriptions.Add(new SortDescription(AlgoPluginViewModel.LevelHeader, ListSortDirection.Descending));
            PluginsView.SortDescriptions.Add(new SortDescription(AlgoPluginViewModel.SortPackageLevelHeader, ListSortDirection.Ascending));
            PluginsView.SortDescriptions.Add(new SortDescription(AlgoPluginViewModel.BotLevelHeader, ListSortDirection.Ascending));

            PluginsView.GroupDescriptions.Add(new PropertyGroupDescription(AlgoPluginViewModel.LevelHeader));
            PluginsView.GroupDescriptions.Add(new PropertyGroupDescription(AlgoPluginViewModel.PackageLevelHeader));

            PluginsView.Filter = new Predicate<object>(Filter);

            NotifyOfPropertyChange(nameof(PluginsView));
        }

        private bool Filter(object obj)
        {
            if (obj is AlgoPluginViewModel vm)
            {
                if (string.IsNullOrEmpty(_filterString))
                    return true;

                return FindMatches(vm.DisplayName) || FindMatches(vm.PackageName);
            }

            return false;
        }

        private bool FindMatches(string str) => str.ToLower().Contains(_filterString.ToLower());
    }
}
