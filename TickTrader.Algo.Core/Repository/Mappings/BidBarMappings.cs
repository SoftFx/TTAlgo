﻿using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    [Serializable]
    public class BidBarMapping : Mapping
    {
        internal BidBarMapping()
        {
            Key = new MappingKey(MappingCollection.BidBarReduction);
            DisplayName = "Bid";
        }


        public override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Bid);
        }
    }


    [Serializable]
    public class BidBarToDoubleMapping : Mapping
    {
        internal BidBarToDoubleMapping()
        {
            Key = new MappingKey(MappingCollection.BidBarReduction, MappingCollection.DefaultBarToDoubleReduction);
            DisplayName = "Bid.Close";
        }

        internal BidBarToDoubleMapping(ReductionKey doubleReductionKey, string doubleReductionDisplayName)
        {
            Key = new MappingKey(MappingCollection.BidBarReduction, doubleReductionKey);
            DisplayName = $"Bid.{doubleReductionDisplayName}";
        }


        public override void MapInput(IPluginSetupTarget target, string inputName, string symbol)
        {
            var doubleReduction = AlgoAssemblyInspector.GetReduction(Key.SecondaryReduction.DescriptorId);
            var doubleReductionInstance = doubleReduction?.CreateInstance<BarToDoubleReduction>() ?? new BarToCloseReduction();
            target.GetFeedStrategy<BarStrategy>().MapInput(inputName, symbol, BarPriceType.Bid, bar => doubleReductionInstance.Reduce(bar));
        }
    }
}