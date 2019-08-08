using System;
using System.Globalization;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator
{
    public class CorrectUriValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Uri.TryCreate(value as string, UriKind.Absolute, out Uri uri))
                return new ValidationResult(true, "");
            else
                return new ValidationResult(false, $"Cannot convert to Url");
        }
    }

    public class CorrectHostValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string specialSymbols = "$-_.+ !*'()";

            if (value is string host)
            {
                if (string.IsNullOrEmpty(host))
                    return new ValidationResult(false, "This field is required");

                foreach (var c in host)
                {
                    if (!char.IsLetterOrDigit(c) && specialSymbols.IndexOf(c) == -1)
                        return new ValidationResult(false, $"An invalid character {c} was found");
                }

                return new ValidationResult(true, "");
            }

            return new ValidationResult(false, "Cannot convert to string");
        }
    }
}
