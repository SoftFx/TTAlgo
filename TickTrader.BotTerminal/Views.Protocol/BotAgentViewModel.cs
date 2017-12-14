using Caliburn.Micro;

namespace TickTrader.BotTerminal
{
    internal class BotAgentViewModel : PropertyChangedBase
    {
        public BotAgentManager Agent { get; }


        public BotAgentViewModel(BotAgentManager agent)
        {
            Agent = agent;
        }
    }
}
