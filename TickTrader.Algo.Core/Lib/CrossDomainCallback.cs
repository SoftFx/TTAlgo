using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Lib
{
    public class CrossDomainCallback<T> : CrossDomainObject
    {
        public CrossDomainCallback()
        {
        }

        public Action<T> Action { get; set; }

        public CrossDomainCallback(Action<T> callbackAction)
        {
            Action = callbackAction;
        }

        public void Invoke(T args)
        {
            Action(args);
        }
    }

    public class CrossDomainCallback : CrossDomainObject
    {
        public CrossDomainCallback()
        {
        }

        public Action Action { get; set; }

        public CrossDomainCallback(Action callbackAction)
        {
            Action = callbackAction;
        }

        public void Invoke()
        {
            Action();
        }
    }
}
