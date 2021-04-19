namespace Machinarium.Var
{
    public static class ValidationExt
    {
        public static Validable<string> MustBeNotEmpty(this Validable<string> property)
        {
            property.AddValidationRule(v => !string.IsNullOrEmpty(v), "Value is required!");
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