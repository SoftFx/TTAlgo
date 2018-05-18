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
    internal class AlgoListViewModel : PropertyChangedBase
    {
        public IObservableList<AlgoItemViewModel> Plugins { get; private set; }


        public AlgoListViewModel(PluginCatalog catalog)
        {
            Plugins = catalog.PluginList
                .Where(p => p.Key.PackageName != "ticktrader.algo.indicators.dll")
                .Select(p => new AlgoItemViewModel(p))
                .AsObservable();
        }
    }


    public class AlgoItemViewModel
    {
        public PluginCatalogItem PluginItem { get; }

        public string Name { get; }

        public string Group { get; }

        public string Description { get; }

        public string Category { get; }


        public AlgoItemViewModel(PluginCatalogItem item)
        {
            PluginItem = item;
            Name = item.Descriptor.UiDisplayName;
            Description = string.Join(Environment.NewLine, item.Descriptor.Description, string.Empty, $"Package {item.Key.PackageName} at {item.Key.PackageLocation}").Trim();
            Category = item.Descriptor.Category;
            switch (item.Descriptor.Type)
            {
                case AlgoTypes.Indicator: Group = "Indicators"; break;
                case AlgoTypes.Robot: Group = "Bot Traders"; break;
                default: Group = "Unknown type"; break;
            }
        }
    }
}
