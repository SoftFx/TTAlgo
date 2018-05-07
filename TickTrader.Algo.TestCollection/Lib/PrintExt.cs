using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.TestCollection
{
    internal static class PrintExt
    {
        public static void PrintPropertiesColumn<T>(this StringBuilder sb, string name, T obj)
        {
            sb.AppendLine($" ------------ {name} ------------");
            PrintPropertiesColumn(sb, obj);
        }

        public static void PrintPropertiesColumn<T>(this StringBuilder sb, T obj)
        {
            var type = typeof(T);
            PrintPropertiesColumn(sb, "", type, obj);
        }

        private static void PrintPropertiesColumn(StringBuilder sb, string prefix, Type type, object obj)
        {
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(type))
            {
                var pNname = descriptor.Name;
                var pValue = descriptor.GetValue(obj);

                if (descriptor.IsPrintPrimitive() || pValue == null)
                    sb.Append(prefix).AppendLine($"{pNname} = {pValue}");
                else
                    PrintPropertiesColumn(sb, prefix + descriptor.Name + ".", descriptor.PropertyType, pValue);
            }
        }

        public static void PrintPropertiesLine<T>(this StringBuilder sb, T obj)
        {
            var type = typeof(T);
            PrintPropertiesLine(sb, type, obj);
        }

        private static void PrintPropertiesLine(StringBuilder sb, Type type, object obj)
        {
            var first = true;

            sb.Append("{");

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(type))
            {
                if (first)
                    first = false;
                else
                    sb.Append(" ");

                var pNname = descriptor.Name;
                var pValue = descriptor.GetValue(obj);

                if (descriptor.IsPrintPrimitive() || pValue == null)
                    sb.Append($"{pNname}={pValue}");
                else
                {
                    sb.Append($"{pNname}=");
                    PrintPropertiesLine(sb, descriptor.PropertyType, pValue);
                }
            }

            sb.Append("}");
        }

        private static bool IsPrintPrimitive(this PropertyDescriptor descriptor)
        {
            return descriptor.PropertyType.IsPrimitive
                || descriptor.PropertyType.IsEnum
                || descriptor.PropertyType == typeof(DateTime)
                || descriptor.PropertyType == typeof(TimeSpan)
                || descriptor.PropertyType == typeof(string);
        }
    }
}
