using System;

namespace TickTrader.Algo.Common.Model.Setup
{
    public static class UiConverter
    {
        static UiConverter()
        {
            Bool = new BoolConverter();
            Int = new IntConverter();
            NullableInt = new NullableIntConverter();
            Double = new DoubleConverter();
            NullableDouble = new NullableDoubleConverter();
            String = new StringConverter();
        }

        public static UiConverter<bool> Bool { get; private set; }
        public static UiConverter<int> Int { get; private set; }
        public static UiConverter<int?> NullableInt { get; private set; }
        public static UiConverter<double> Double { get; private set; }
        public static UiConverter<double?> NullableDouble { get; private set; }
        public static UiConverter<string> String { get; private set; }


        internal class BoolConverter : UiConverter<bool>
        {
            public override bool Parse(string str, out GuiModelMsg error)
            {
                error = null;

                try
                {
                    if (!string.IsNullOrWhiteSpace(str))
                        return Boolean.Parse(str);
                }
                catch (FormatException)
                {
                    error = new GuiModelMsg(MsgCodes.NotBoolean);
                }

                return false;
            }

            public override string ToString(bool val)
            {
                return val.ToString();
            }

            public override bool FromObject(object objVal, out bool result)
            {
                try
                {
                    result = System.Convert.ToBoolean(objVal);
                    return true;
                }
                catch
                {
                    result = false;
                    return false;
                }
            }
        }


        internal class StringConverter : UiConverter<string>
        {
            public override string Parse(string str, out GuiModelMsg error)
            {
                error = null;
                return str;
            }

            public override string ToString(string val)
            {
                return val;
            }

            public override bool FromObject(object objVal, out string result)
            {
                result = objVal?.ToString() ?? "";
                return true;
            }
        }

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
                return val.ToString();
            }

            public override bool FromObject(object objVal, out int result)
            {
                try
                {
                    result = System.Convert.ToInt32(objVal);
                    return true;
                }
                catch (Exception)
                {
                    result = 0;
                    return false;
                }
            }
        }

        internal class NullableIntConverter : UiConverter<int?>
        {
            public override int? Parse(string str, out GuiModelMsg error)
            {
                error = null;
                try
                {
                    return string.IsNullOrWhiteSpace(str) ? default(int?) : int.Parse(str);
                }
                catch (FormatException)
                {
                    error = new GuiModelMsg(MsgCodes.NotInteger);
                }
                catch (OverflowException)
                {
                    error = new GuiModelMsg(MsgCodes.NumberOverflow);
                }
                return default(int?);
            }

            public override string ToString(int? val)
            {
                return val.ToString();
            }

            public override bool FromObject(object objVal, out int? result)
            {
                try
                {
                    if (objVal == null)
                    {
                        result = default(int?);
                    }
                    else if (objVal is string && string.IsNullOrWhiteSpace((string)objVal))
                    {
                        result = default(int?);
                    }
                    else
                    {
                        result = System.Convert.ToInt32(objVal);
                    }

                    return true;
                }
                catch (Exception)
                {
                    result = default(int?);
                    return false;
                }
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
                return val.ToString("R");
            }

            public override bool FromObject(object objVal, out double result)
            {
                try
                {
                    result = System.Convert.ToDouble(objVal);
                    return true;
                }
                catch (Exception)
                {
                    result = 0;
                    return false;
                }
            }
        }

        internal class NullableDoubleConverter : UiConverter<double?>
        {
            public override double? Parse(string str, out GuiModelMsg error)
            {
                error = null;
                try
                {
                    return string.IsNullOrWhiteSpace(str) ? default(double?) : double.Parse(str);
                }
                catch (FormatException)
                {
                    error = new GuiModelMsg(MsgCodes.NotDouble);
                }
                catch (OverflowException)
                {
                    error = new GuiModelMsg(MsgCodes.NumberOverflow);
                }
                return default(double?);
            }

            public override string ToString(double? val)
            {
                return val.ToString();
            }

            public override bool FromObject(object objVal, out double? result)
            {
                try
                {
                    if (objVal == null)
                    {
                        result = default(double?);
                    }
                    else if (objVal is string && string.IsNullOrWhiteSpace((string)objVal))
                    {
                        result = default(double?);
                    }
                    else
                    {
                        result = System.Convert.ToDouble(objVal);
                    }

                    return true;
                }
                catch (Exception)
                {
                    result = default(double?);
                    return false;
                }
            }
        }
    }

    public abstract class UiConverter<T>
    {
        public abstract T Parse(string str, out GuiModelMsg error);
        public abstract string ToString(T val);
        public abstract bool FromObject(object objVal, out T result);
    }
}
