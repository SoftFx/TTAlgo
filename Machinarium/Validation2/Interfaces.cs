using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Validation2
{
    public interface IValidationContext
    {
        IVarSet<object> Errors { get; }
    }

    public interface IValidationSource
    {
        Var<object> Error { get; }
    }
}
