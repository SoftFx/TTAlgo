using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public interface IVar : IDisposable
    {
        event Action<bool> Changed;
    }

    public interface IVar<T> : IVar
    {
        T Value { get; set; }
    }
}
