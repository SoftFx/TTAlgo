using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.CoreV1
{
    public sealed class PositionAccessor : NetPosition
    {
        private readonly SymbolAccessor _symbol;
        private readonly string _symbolName;


        public PositionInfo Info { get; private set; }


        internal event Action<PositionAccessor> Changed;


        internal PositionAccessor(string symbolName, SymbolAccessor symbolInfo, PositionInfo posInfo = null)
        {
            _symbolName = symbolName;
            _symbol = symbolInfo;

            Update(posInfo);
        }

        internal void Update(PositionInfo info)
        {
            Info = info ?? new PositionInfo() { Symbol = _symbolName };

            Changed?.Invoke(this);
        }

        internal PositionAccessor Clone() => new PositionAccessor(_symbolName, _symbol, Info);


        string NetPosition.Id => Info.Id;

        double NetPosition.Price => Info.Price;

        string NetPosition.Symbol => Info.Symbol;

        OrderSide NetPosition.Side => Info.Side.ToApiEnum();

        DateTime? NetPosition.Modified => Info.Modified?.ToDateTime();

        double NetPosition.Swap => Info.Swap;

        double NetPosition.Commission => Info.Commission;

        double NetPosition.Volume => Info.Volume / _symbol?.Info?.LotSize ?? 1;

        double NetPosition.Margin => ProcessResponse(Info.Calculator?.Margin?.Calculate(Info));

        double NetPosition.Profit => ProcessResponse(Info.Calculator?.Profit?.Calculate(Info));


        private static double ProcessResponse(ICalculateResponse<double> response)
        {
            return response != null && response.IsCompleted ? response.Value : double.NaN;
        }
    }
}
