using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.EntityModel
{
    public static class FieldExt
    {
        private static IntConverter intConverterInstance = new IntConverter();

        public static FieldProxy<string> AsString(this Field<int> field)
        {
            return field.AddConverter(intConverterInstance);
        }
    }
}
