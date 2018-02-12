using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class OrderAmountModel
    {
        private static decimal[] predefinedLotsCollection = new decimal[] { 0.01M, 0.02M, 0.05M, 0.1M, 0.2M, 0.5M, 1, 2, 5, 10, 20, 50, 100, 200, 500 };

        public OrderAmountModel(SymbolEntity symbolDescriptor)
        {
            LotSize = (decimal)symbolDescriptor.RoundLot;
            if (LotSize != 0)
            {
                MinAmount = (decimal)symbolDescriptor.MinTradeVolume / LotSize;
                MaxAmount = (decimal)symbolDescriptor.MaxTradeVolume / LotSize;
                AmountStep = (decimal)symbolDescriptor.TradeVolumeStep / LotSize;
            }
            else
            {
                MinAmount = (decimal)symbolDescriptor.MinTradeVolume;
                MaxAmount = (decimal)symbolDescriptor.MaxTradeVolume;
                AmountStep = (decimal)symbolDescriptor.TradeVolumeStep;
            }

        }

        public decimal MaxAmount { get; private set; }
        public decimal MinAmount { get; private set; }
        public decimal AmountStep { get; private set; }
        public decimal LotSize { get; private set; }

        public List<decimal> GetPredefined()
        {
            return predefinedLotsCollection.Where(ValidateAmount).ToList();
        }

        public bool ValidateAmount(decimal amount)
        {
            return amount <= MaxAmount && amount >= MinAmount
                && (amount / AmountStep) % 1 == 0;
        }
    }
}
