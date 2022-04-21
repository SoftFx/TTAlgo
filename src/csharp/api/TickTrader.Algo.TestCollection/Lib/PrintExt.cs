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
            PrintPropertiesLine(sb, type, obj, false);
        }

        public static void PrintPropertiesColumnOfLines<T>(this StringBuilder sb, T obj)
        {
            var type = typeof(T);

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(type))
            {
                var pName = descriptor.Name;
                var pValue = descriptor.GetValue(obj);
                var pType = descriptor.PropertyType;

                if (sb.Length > 0)
                    sb.AppendLine();

                if (descriptor.IsPrintPrimitive() || pValue == null)
                    sb.Append($"{pName} = {pValue}");
                else
                {
                    sb.Append(pName).Append(": ");
                    PrintPropertiesLine(sb, pType, pValue, false);
                }
            }
        }

        private static void PrintPropertiesLine(StringBuilder sb, Type type, object obj, bool wrapp)
        {
            var first = true;

            if (wrapp)
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
                    PrintPropertiesLine(sb, descriptor.PropertyType, pValue, true);
                }
            }

            if (wrapp)
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
