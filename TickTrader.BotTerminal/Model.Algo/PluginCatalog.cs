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

        public IVarSet<PluginKey, PluginInfo> Indicators { get; }

        public IVarSet<PluginKey, PluginInfo> BotTraders { get; }


        public PluginCatalog(IAlgoLibrary algoLibrary)
        {
            _algoLibrary = algoLibrary;

            _logger = NLog.LogManager.GetCurrentClassLogger();

            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            foreach (var plugin in _algoLibrary.GetPlugins())
            {
                _plugins.Add(plugin.Key, plugin);
            }

            Indicators = _plugins.Where((k, p) => p.Descriptor.Type == AlgoTypes.Indicator);
            BotTraders = _plugins.Where((k, p) => p.Descriptor.Type == AlgoTypes.Robot);

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
