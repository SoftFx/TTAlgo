using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Validation2
{
    public class Validator : IDisposable, IValidationContext
    {
        private List<IValidationContext> _contexts = new List<IValidationContext>();
        private List<ValidationRule> _rules = new List<ValidationRule>();

        public Validator()
        {
        }

        public IVarSet<object> Errors { get; }

        public void AddRule(BoolVar condition, object error)
        {
            AddSource(new ValidationRule(condition, error));
        }

        public void AddSource(IValidationContext src)
        {
            _contexts.Add(src);
            src.Errors.Updated += Errors_Updated;
        }

        public void AddSource(IValidationSource src)
        {
            src.Error.Changed += Error_Changed;
        }

        private void Error_Changed()
        {

        }

        public void RemoveSource(IValidationContext src)
        {
            _contexts.Add(src);
            src.Errors.Updated -= Errors_Updated;
        }

        private void Errors_Updated(SetUpdateArgs<object> args)
        {

        }

        private void Condition_Changed()
        {

        }

        public void Dispose()
        {
        }

        private class ValidationRule : IValidationSource
        {
            public ValidationRule(BoolVar condition, object error)
            {
                Condition = condition;
                Error = Condition.Convert<bool, object>(c => c ? null : Error);
            }

            public BoolVar Condition { get; }
            public Var<object> Error { get; }
        }
    }
}
