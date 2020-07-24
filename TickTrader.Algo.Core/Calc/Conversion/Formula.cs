using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Calc.Conversion
{
    internal interface IConversionFormula
    {
        double Value { get; }
        CalcErrorCodes ErrorCode { get; }

        void AddUsage();
        void RemoveUsage();

        event Action ValChanged;
    }

    internal class NoConvertion : IConversionFormula
    {
        public double Value => 1;
        public CalcErrorCodes ErrorCode => CalcErrorCodes.None;

        public event Action ValChanged { add { } remove { } }

        public void AddUsage() { }
        public void RemoveUsage() { }
    }

    internal abstract class UsageAwareFormula : IConversionFormula
    {
        private double _val;

        public double Value
        {
            get => _val;
            set
            {
                if (_val != value)
                {
                    _val = value;
                    ValChanged?.Invoke();
                }
            }
        }

        public int UsageCount { get; private set; }
        public CalcErrorCodes ErrorCode { get; protected set; }
        public event Action ValChanged;

        protected abstract void Attach();
        protected abstract void Deattach();

        public void AddUsage()
        {
            if (UsageCount <= 0)
                Attach();

            UsageCount++;
        }

        public void RemoveUsage()
        {
            UsageCount--;

            if (UsageCount <= 0)
                Deattach();
        }
    }

    internal abstract class ComplexConversion : UsageAwareFormula
    {
        public IConversionFormula SrcFromula { get; set; }
        public SymbolMarketNode SrcSymbol { get; set; }

        protected abstract double GetValue();

        protected override void Attach()
        {
            if (SrcFromula != null)
            {
                SrcFromula.AddUsage();
                SrcFromula.ValChanged += SrcFromula_ValChanged;
            }

            SrcSymbol.Changed += SrcSymbol_Changed;

            Value = GetValue();
        }

        protected override void Deattach()
        {
            if (SrcFromula != null)
            {
                SrcFromula.RemoveUsage();
                SrcFromula.ValChanged -= SrcFromula_ValChanged;
            }

            SrcSymbol.Changed -= SrcSymbol_Changed;
        }

        private void SrcSymbol_Changed()
        {
            Value = GetValue();
        }

        private void SrcFromula_ValChanged()
        {
            Value = GetValue();
        }

        protected bool CheckBid()
        {
            if (!SrcSymbol.HasBid)
            {
                ErrorCode = CalcErrorCodes.OffCrossQuote;
                return false;
            }
            return true;
        }

        protected bool CheckAsk()
        {
            if (!SrcSymbol.HasAsk)
            {
                ErrorCode = CalcErrorCodes.OffCrossQuote;
                return false;
            }
            return true;
        }

        protected bool CheckSrcFormula()
        {
            var error = SrcFromula.ErrorCode;

            if (error != CalcErrorCodes.None)
            {
                ErrorCode = error;
                return false;
            }

            return true;
        }
    }

    internal class GetAsk : ComplexConversion
    {
        protected override double GetValue()
        {
            if (CheckAsk())
            {
                ErrorCode = CalcErrorCodes.None;
                return SrcSymbol.Ask;
            }
            return 0;
        }
    }

    internal class GetBid : ComplexConversion
    {
        protected override double GetValue()
        {
            if (CheckBid())
            {
                ErrorCode = CalcErrorCodes.None;
                return SrcSymbol.Bid;
            }
            return 0;
        }
    }

    internal class GetInvertedAsk : ComplexConversion
    {
        protected override double GetValue()
        {
            if (CheckAsk())
            {
                ErrorCode = CalcErrorCodes.None;
                return 1 / SrcSymbol.Ask;
            }
            return 0;
        }
    }

    internal class GetInvertedBid : ComplexConversion
    {
        protected override double GetValue()
        {
            if (CheckBid())
            {
                ErrorCode = CalcErrorCodes.None;
                return 1 / SrcSymbol.Bid;
            }
            return 0;
        }
    }

    internal class MultByBid : ComplexConversion
    {
        protected override double GetValue()
        {
            if (CheckBid() && CheckSrcFormula())
            {
                ErrorCode = CalcErrorCodes.None;
                return SrcFromula.Value * SrcSymbol.Bid;
            }
            return 0;
        }
    }

    internal class MultByAsk : ComplexConversion
    {
        protected override double GetValue()
        {
            if (CheckAsk() && CheckSrcFormula())
            {
                ErrorCode = CalcErrorCodes.None;
                return SrcFromula.Value * SrcSymbol.Ask;
            }
            return 0;
        }
    }

    internal class DivByBid : ComplexConversion
    {
        protected override double GetValue()
        {
            if (CheckBid() && CheckSrcFormula())
            {
                ErrorCode = CalcErrorCodes.None;
                return SrcFromula.Value / SrcSymbol.Bid;
            }
            return 0;
        }
    }

    internal class DivByAsk : ComplexConversion
    {
        protected override double GetValue()
        {
            if (CheckAsk() && CheckSrcFormula())
            {
                ErrorCode = CalcErrorCodes.None;
                return SrcFromula.Value / SrcSymbol.Ask;
            }
            return 0;
        }
    }

    internal class ConversionError : IConversionFormula
    {
        public ConversionError(CalcErrorCodes errorCode)
        {
            ErrorCode = errorCode;
        }

        public double Value => throw new InvalidOperationException("Conversion error: " + ErrorCode + "!");
        public CalcErrorCodes ErrorCode { get; }
        public event Action ValChanged { add { } remove { } }

        public void AddUsage() { }
        public void RemoveUsage() { }
    }
}
