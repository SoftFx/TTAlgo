using System.Collections.Generic;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class AdvancedViewModel : BaseContentViewModel
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly RefreshCounter _refreshManager;
        private readonly string _keyPath;

        private string _selectPath;

        private RegistryManager _registry;

        public bool NewCurrentAgent { get; private set; } = false;

        public AdvancedViewModel(RegistryManager registry, RefreshCounter _refManager = null) : base(nameof(AdvancedViewModel))
        {
            _registry = registry;
            _refreshManager = _refManager;
            _selectPath = registry.CurrentAgent.Path;

            _keyPath = $"{nameof(AdvancedViewModel)} {nameof(SelectPath)}";
        }

        public IEnumerable<string> AgentPaths => _registry.AgentNodes.Select(n => n.Path);

        public string SelectPath
        {
            get => _selectPath;

            set
            {
                _logger.Info(GetChangeMessage(_keyPath, _selectPath, value));
                _selectPath = value;

                _refreshManager?.CheckUpdate(value, _registry.OldAgent.Path, _keyPath);

                NewCurrentAgent = _registry.OldAgent.Path != value;
                OnPropertyChanged(nameof(NewCurrentAgent));
            }
        }
    }
}
