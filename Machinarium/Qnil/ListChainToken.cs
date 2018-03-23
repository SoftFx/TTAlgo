using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.Qnil
{
    internal class ListChainToken<T> : IVarList<T>
    {
        private IVarList<T> src;

        public ListChainToken(IVarList<T> src)
        {
            this.src = src;
        }

        public IVarList<T> Src { get { return src; } }
        public IReadOnlyList<T> Snapshot { get { return src.Snapshot; } }
        public event ListUpdateHandler<T> Updated { add { src.Updated += value; } remove { src.Updated -= value; } }

        public void Dispose()
        {
            src.Dispose();
        }
    }
}
