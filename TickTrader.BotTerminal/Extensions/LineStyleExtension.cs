using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal static class LineStylesExtension
    {
        public static double[] ToStrokeDashArray(this Metadata.Types.LineStyle lineStyle)
        {
            switch (lineStyle)
            {
                case Metadata.Types.LineStyle.Dots:
                    return new double[] { 1.5, 2 };
                case Metadata.Types.LineStyle.DotsRare:
                    return new double[] { 1.5, 4 };
                case Metadata.Types.LineStyle.DotsVeryRare:
                    return new double[] { 1.5, 8 };
                case Metadata.Types.LineStyle.Lines:
                    return new double[] { 8, 6 };
                case Metadata.Types.LineStyle.LinesDots:
                    return new double[] { 5, 2, 1.5, 2 };
                default: return new double[0];
            }
        }
    }
}
