using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.EntityModel
{
    public interface PropertyConverter<TSrc, TDst>
    {
        TDst Convert(TSrc val, out object error);
        TSrc ConvertBack(TDst value);
    }

    public enum GenericConvertErrors
    {
        InvalidInteger,
        InvalidDouble
    }

    public class IntConverter : PropertyConverter<string, int>
    {
        public int Convert(string val, out object error)
        {
            int result;
            if (!int.TryParse(val, out result))
                error = GenericConvertErrors.InvalidInteger;
            else
                error = null;
            return result;
        }

        public string ConvertBack(int value)
        {
            return value.ToString();
        }
    }

    public class DoubleConverter : PropertyConverter<string, double>
    {
        public double Convert(string val, out object error)
        {
            double result;
            if (!double.TryParse(val, out result))
                error = GenericConvertErrors.InvalidDouble;
            else
                error = null;
            return result;
        }

        public string ConvertBack(double value)
        {
            return value.ToString();
        }
    }
}
