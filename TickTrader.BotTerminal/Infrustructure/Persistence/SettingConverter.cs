using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Infrustructure.Persistence
{
    public static class SettingConverter
    {
        public static bool ToInt32(string str, out int val)
        {
            return int.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
        }

        public static string ToString(int val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        public static bool ToDouble(string str, out double val)
        {
            return double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out val);
        }

        public static string ToString(double val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        public static bool ToBool(string str, out bool val)
        {
            return bool.TryParse(str, out val);
        }

        public static string ToString(bool val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        public static string Serialize(object val, Type targetType)
        {
            switch (val)
            {
                case string strVal: return strVal;
                case int intVal: return ToString(intVal);
                case double dblVal: return ToString(dblVal);
                case bool boolVal: return ToString(boolVal);
            }

            throw new Exception("SettingConverter does not support type " + val.GetType());
        }

        public static bool Deserialize(string strVal, Type targetType, out object val)
        {
            if (targetType == typeof(string))
            {
                val = strVal;
                return true;
            }
            else if (targetType == typeof(int))
            {
                var result = ToInt32(strVal, out var valInt);
                val = valInt;
                return result;
            }
            else if (targetType == typeof(double))
            {
                var result = ToDouble(strVal, out var valDouble);
                val = valDouble;
                return result;
            }
            else if (targetType == typeof(bool))
            {
                var result = ToBool(strVal, out var valBool);
                val = valBool;
                return result;
            }

            throw new Exception("SettingConverter does not support type " + targetType);
        }
    }
}
