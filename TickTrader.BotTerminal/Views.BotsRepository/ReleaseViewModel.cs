using System.Collections.ObjectModel;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class ReleaseViewModel
    {
        public string Name { get; init; }

        public ObservableCollection<BotMetainfoViewModel> Plugins { get; } = new();
    }
}
