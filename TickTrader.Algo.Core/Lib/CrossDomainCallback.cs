using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class CrossDomainCallback<T> : CrossDomainObject
    {
        private Action<T> _callbackAction;

        public CrossDomainCallback(Action<T> callbackAction)
        {
            _callbackAction = callbackAction;
        }

        public void Invoke(T args)
        {
            _callbackAction(args);
        }
    }
}
