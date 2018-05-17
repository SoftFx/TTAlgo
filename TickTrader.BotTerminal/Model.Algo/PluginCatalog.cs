using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class PluginCatalog
    {
        private IAlgoLibrary _algoLibrary;
        private Logger _logger;
        private VarDictionary<PluginKey, PluginInfo> _plugins;


        public IVarSet<PluginKey, PluginInfo> AllPlugins => _plugins;

        public IVarList<PluginCatalogItem> PluginList { get; }

        public IVarList<PluginCatalogItem> Indicators { get; }

        public IVarList<PluginCatalogItem> BotTraders { get; }


        public PluginCatalog(IAlgoLibrary algoLibrary)
        {
            _algoLibrary = algoLibrary;

            _logger = NLog.LogManager.GetCurrentClassLogger();

            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            foreach (var plugin in _algoLibrary.GetPlugins())
            {
                _plugins.Add(plugin.Key, plugin);
            }

            PluginList = _plugins.OrderBy((k, p) => p.Descriptor.UiDisplayName).Select(info => new PluginCatalogItem(info));
            Indicators = PluginList.Where(i => i.Descriptor.Type == AlgoTypes.Indicator);
            BotTraders = PluginList.Where(i => i.Descriptor.Type == AlgoTypes.Robot);

            _algoLibrary.PluginAdded += LibraryOnPluginAdded;
            _algoLibrary.PluginReplaced += LibraryOnPluginReplaced;
            _algoLibrary.PluginRemoved += LibraryOnPluginRemoved;
        }


        private void LibraryOnPluginAdded(PluginInfo plugin)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    _plugins.Add(plugin.Key, plugin);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });
        }

        private void LibraryOnPluginReplaced(PluginInfo plugin)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    _plugins[plugin.Key] = plugin;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });
        }

        private void LibraryOnPluginRemoved(PluginInfo plugin)
        {
            Execute.OnUIThread(() =>
            {
                try
                {
                    _plugins.Remove(plugin.Key);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            });
        }
    }
}
