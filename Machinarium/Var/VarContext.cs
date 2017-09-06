using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Var
{
    public class VarContext : List<IDisposable>, IDisposable
    {
        [ThreadStatic]
        private static VarContext current;

        internal VarContext Current => current;

        internal static void AddIfContextExist(IDisposable disposable)
        {
            current?.Add(disposable);
        }

        public VarContext(bool cascade = true)
        {
            if (cascade)
                AddIfContextExist(this);
        }

        public VarContext(Action contextInitAction)
        {
            var old = current;
            current = this;
            try
            {
                contextInitAction();
            }
            finally
            {
                current = old;
            }
        }

        public void Dispose()
        {
            foreach (var d in this)
                d.Dispose();

            Clear();
        }
    }
}
