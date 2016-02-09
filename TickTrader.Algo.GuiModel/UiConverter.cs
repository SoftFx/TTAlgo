using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.GuiModel
{
    public static class UiConverter
    {
        static UiConverter()
        {
            Int = new IntConverter();
            Double = new DoubleConverter();
        }

        public static UiConverter<int> Int { get; private set; }
        public static UiConverter<double> Double { get; private set; }

        internal class IntConverter : UiConverter<int>
        {
            public override int Parse(string str, out GuiModelMsg error)
            {
                error = null;
                try
                {
                    return int.Parse(str);
                }
                catch (FormatException)
                {
                    error = new GuiModelMsg(MsgCodes.NotInteger);
                }
                catch (OverflowException)
                {
                    error = new GuiModelMsg(MsgCodes.NumberOverflow);
                }
                return 0;
            }

            public override string ToString(int val)
            {
                return ((int)val).ToString();
            }
        }

        internal class DoubleConverter : UiConverter<double>
        {
            public override double Parse(string str, out GuiModelMsg error)
            {
                error = null;
                try
                {
                    return double.Parse(str);
                }
                catch (FormatException)
                {
                    error = new GuiModelMsg(MsgCodes.NotDouble);
                }
                catch (OverflowException)
                {
                    error = new GuiModelMsg(MsgCodes.NumberOverflow);
                }
                return 0;
            }

            public override string ToString(double val)
            {
                return ((double)val).ToString("R");
            }
        }
    }

    public abstract class UiConverter<T>
    {
        public abstract T Parse(string str, out GuiModelMsg error);
        public abstract string ToString(T val);
    }
}
