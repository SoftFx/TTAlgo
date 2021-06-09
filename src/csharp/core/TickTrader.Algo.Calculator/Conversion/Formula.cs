using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Calculator
{
    public interface IConversionFormula
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

        internal ComplexConversion() { }

        internal ComplexConversion(SymbolMarketNode srcSymbol) : this(srcSymbol, null) { }


        internal ComplexConversion(SymbolMarketNode srcSymbol, IConversionFormula formula)
        {
            SrcSymbol = srcSymbol;
            SrcFromula = formula;

            Value = GetValue();
        }

        protected override void Attach()
        {
            if (SrcFromula != null)
            {
                SrcFromula.AddUsage();
                SrcFromula.ValChanged += SrcFromula_ValChanged;
            }

            SrcSymbol.SymbolInfo.RateUpdated += SrcSymbol_Changed;

            Value = GetValue();
        }

        protected override void Deattach()
        {
            if (SrcFromula != null)
            {
                SrcFromula.RemoveUsage();
                SrcFromula.ValChanged -= SrcFromula_ValChanged;
            }

            SrcSymbol.SymbolInfo.RateUpdated -= SrcSymbol_Changed;
        }

        private void SrcSymbol_Changed(ISymbolInfo smb)
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
        internal GetAsk(SymbolMarketNode smb) : base(smb) { }

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
        internal GetBid(SymbolMarketNode smb) : base(smb) { }

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
        internal GetInvertedAsk(SymbolMarketNode smb) : base(smb) { }

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
        internal GetInvertedBid(SymbolMarketNode smb) : base(smb) { }

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
        internal MultByBid(SymbolMarketNode node, IConversionFormula formula) : base(node, formula) { }

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
        internal MultByAsk(SymbolMarketNode node, IConversionFormula formula) : base(node, formula) { }

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
        internal DivByBid(SymbolMarketNode node, IConversionFormula formula) : base(node, formula) { }

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
        internal DivByAsk(SymbolMarketNode node, IConversionFormula formula) : base(node, formula) { }

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

        public double Value => throw new InvalidOperationException($"Conversion error: {ErrorCode }!");
        public CalcErrorCodes ErrorCode { get; }
        public event Action ValChanged { add { } remove { } }

        public void AddUsage() { }
        public void RemoveUsage() { }
    }
}
