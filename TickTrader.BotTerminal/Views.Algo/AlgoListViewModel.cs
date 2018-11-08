using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public enum AlgoListViewType
    {
        Plugins = 0,
        Packages = 1,
    }


    internal class AlgoListViewModel : PropertyChangedBase
    {
        private AlgoAgentViewModel _selectedAgent;
        private AlgoListViewType _selectedViewType;


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
                NotifyOfPropertyChange(nameof(Packages));
                NotifyOfPropertyChange(nameof(Plugins));
            }
        }

        public IObservableList<AlgoPackageViewModel> Packages => SelectedAgent.PackageList;

        public IObservableList<AlgoPluginViewModel> Plugins => SelectedAgent.Plugins.Where(p => p.Key.PackageName != "ticktrader.algo.indicators.dll").AsObservable();

        public AlgoListViewType[] AvaliableViewTypes { get; }

        public AlgoListViewType SelectedViewType
        {
            get { return _selectedViewType; }
            set
            {
                if (_selectedViewType == value)
                    return;

                _selectedViewType = value;
                NotifyOfPropertyChange(nameof(SelectedViewType));
            }
        }


        public AlgoListViewModel(AlgoEnvironment algoEnv)
        {
            AvailableAgents = algoEnv.Agents.AsObservable();
            SelectedAgent = AvailableAgents.First();

            AvaliableViewTypes = Enum.GetValues(typeof(AlgoListViewType)).Cast<AlgoListViewType>().ToArray();
            SelectedViewType = AlgoListViewType.Plugins;
        }
    }
}
