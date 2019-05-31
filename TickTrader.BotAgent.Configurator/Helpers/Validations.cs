using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TickTrader.BotAgent.Configurator
{
    public class ValidationString : ValidationRule
    {
        public int MinLength { get; set; } = 0;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if ((value as string).Length < MinLength)
                return new ValidationResult(false, $"String length less than {MinLength}");

            return new ValidationResult(true, "");
        }
    }

    public class ValidationPort : ValidationRule
    {
        public int MinValue { get; set; } = 0;

        public int MaxValue { get; set; } = 0;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value as string, out int num))
            {
                if (num >= MinValue && num <= MaxValue)
                    return new ValidationResult(true, "");
                else
                    return new ValidationResult(false, $"Number must be between {MinValue} to {MaxValue}");
            }
            else
                return new ValidationResult(false, $"Cannot convert to number");
        }
    }

    public class ValidationUri : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Uri.TryCreate(value as string, UriKind.Absolute, out Uri uri))
                return new ValidationResult(true, "");
            else
                return new ValidationResult(false, $"Cannot convert to Uri");
        }
    }
}
