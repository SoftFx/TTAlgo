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
                .Where(p => p.Key.PackageName != "TickTrader.Algo.Indicators")
                .Select(p => new AlgoItemViewModel(p.Info))
                .AsObservable();
        }
    }


    public class AlgoItemViewModel
    {
        public PluginInfo PluginInfo { get; }

        public string Name { get; }

        public string Group { get; }

        public string Description { get; }

        public string Category { get; }


        public AlgoItemViewModel(PluginInfo plugin)
        {
            PluginInfo = plugin;
            Name = plugin.Descriptor.UiDisplayName;
            Description = string.Join(Environment.NewLine, plugin.Descriptor.Description, string.Empty, $"Package {plugin.Key.PackageName} at {plugin.Key.PackageLocation}").Trim();
            Category = plugin.Descriptor.Category;
            var type = plugin.Descriptor.Type;
            if (type == AlgoTypes.Indicator)
                Group = "Indicators";
            else if (type == AlgoTypes.Robot)
                Group = "Bot Traders";
            else
                Group = "Unknown type";
        }
    }
}
