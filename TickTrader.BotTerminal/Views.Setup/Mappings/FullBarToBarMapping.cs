using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public class FullBarToBarMapping : SymbolMapping
    {
        private FullBarToBarReduction _reduction;


        internal FullBarToBarMapping(string name, FullBarToBarReduction reduction)
            : base(name)
        {
            _reduction = reduction;
        }


        internal override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<BarStrategy>().MapInput<Bar>(inputName, symbol, MapValue);
        }


        private BarEntity MapValue(BarEntity bidBar, BarEntity askBar)
        {
            var res = new BarEntity
            {
                OpenTime = bidBar.OpenTime,
                CloseTime = bidBar.CloseTime,
            };
            _reduction.Reduce(bidBar, askBar, res);
            return res;
        }
    }
}
