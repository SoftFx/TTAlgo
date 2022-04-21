using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Machinarium.Var
{
    public class ValidationRange
    {
        public double Min { get; private set; }

        public double Max { get; private set; }


        public ValidationRange(double min, double max) => Update(min, max);


        public void Update(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }


    public static class ValidationExt
    {
        public static Validable<T> MustContainItem<T>(this Validable<T> prop, List<T> items)
        {
            prop.AddValidationRule((v) => items.Contains(v), $"Selected item not found!");

            return prop;
        }

        public static Validable<string> MustBeNotEmpty(this Validable<string> property)
        {
            property.AddValidationRule(v => !string.IsNullOrEmpty(v), "Value is required!");
            return property;
        }


        public static IntValidable AddValidationRange(this IntValidable prop, int min, int max)
        {
            bool Rule(int val) => min <= val && val <= max;

            var message = $"Value must be in range {min} to {max}";

            prop.AddValidationRule(Rule, message);

            return prop;
        }


        public static DoubleValidable MustBePositive(this DoubleValidable property)
        {
            property.AddValidationRule(v => v >= 0.0, $"Value must be positive!");

            return property;
        }

        public static DoubleValidable AddValidationRange(this DoubleValidable property, ValidationRange range, bool includeLeftBound = true)
        {
            bool Rule(double val) => includeLeftBound ? (val >= range.Min && val <= range.Max) : (val > range.Min && val <= range.Max);


            var message = $"Value must be in range {(includeLeftBound ? "[" : "(")}{range.Min:R}..{range.Max:R}]";

            property.AddValidationRule(Rule, message);

            return property;
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