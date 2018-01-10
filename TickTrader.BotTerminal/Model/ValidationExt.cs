using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal static class ValidationExt
    {
        public static void MustBeNotEmpy(this Validable<string> property)
        {
            property.AddValidationRule(v => !string.IsNullOrEmpty(v), "Value is required!");
        }

        public static BoolVar IsValid<T>(this IValidable<T> property)
        {
            return property.ErrorVar.Check(string.IsNullOrEmpty);
        }

        public static BoolVar IsEmpty(this Var<string> property)
        {
            return property.Check(string.IsNullOrEmpty);
        }
    }
}
