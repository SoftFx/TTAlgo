using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.EntityModel
{
    public interface ValidationRule<T>
    {
        object Validate(T value);
    }

    public class GenericValidationRule<T> : ValidationRule<T>
    {
        private Func<T, object> validationFunc;

        public GenericValidationRule(Func<T, object> validationFunc)
        {
            this.validationFunc = validationFunc;
        }

        object ValidationRule<T>.Validate(T value)
        {
            return validationFunc(value);
        }
    }
}
