using Caliburn.Micro;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class BotManagerViewModel : PropertyChangedBase
    {
        private BotManager _botManager;
        private WindowManager _wndManager;


        public IDynamicListSource<BotControlViewModel> Bots { get; }


        public BotManagerViewModel(BotManager botManager, WindowManager wndManager)
        {
            _botManager = botManager;
            _wndManager = wndManager;

            Bots = botManager.Bots.OrderBy((id, bot) => id).Select(b => new BotControlViewModel(b, _wndManager, false, false));
        }
    }
}
