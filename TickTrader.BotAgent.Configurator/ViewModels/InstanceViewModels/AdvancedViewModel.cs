using System.Collections.Generic;
using System.Linq;

namespace TickTrader.BotAgent.Configurator
{
    public class AdvancedViewModel : BaseContentViewModel
    {
        private readonly RefreshCounter _refreshManager;
        private readonly string _keyPath;

        private string _selectPath;

        private RegistryManager _registry;

        public IEnumerable<string> AgentPaths => _registry.ServerNodes.Select(n => n.FolderPath);

        public bool NewCurrentAgent { get; private set; } = false;

        public AdvancedViewModel(RegistryManager registry, RefreshCounter refManager = null) : base(nameof(AdvancedViewModel))
        {
            _registry = registry;
            _refreshManager = refManager;
            _selectPath = registry.CurrentServer.FolderPath;

            _keyPath = $"{nameof(AdvancedViewModel)} {nameof(SelectPath)}";
        }

        public string SelectPath
        {
            get => _selectPath;

            set
            {
                _selectPath = value;

                NewCurrentAgent = _registry.OldServer.FolderPath != value;
                OnPropertyChanged(nameof(NewCurrentAgent));
            }
        }
    }
}
