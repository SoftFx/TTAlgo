using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public abstract class SymbolMapping
    {
        public string Name { get; }


        internal SymbolMapping(string name)
        {
            Name = name;
        }


        internal abstract void MapInput(IPluginSetupTarget target, string inputName, string symbol);
    }
}
