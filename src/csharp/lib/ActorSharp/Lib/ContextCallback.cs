using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ActorSharp.Lib
{
    public class ContextCallback
    {
        private SynchronizationContext _context;
        private Action _callbackAction;

        public ContextCallback(Action action)
        {
            _callbackAction = action ?? throw new ArgumentNullException("action");
            _context = SynchronizationContext.Current ?? throw new Exception("Synchronization context is required!");
        }

        public void Invoke()
        {
            _context.Post(ExecActionInContext, _callbackAction);
        }

        private void ExecActionInContext(object state)
        {
            _callbackAction();
        }
    }

    public class ContextCallback<T>
    {
        private SynchronizationContext _context;
        private Action<T> _callbackAction;

        public ContextCallback(Action<T> action)
        {
            _callbackAction = action ?? throw new ArgumentNullException("action");
            _context = SynchronizationContext.Current ?? throw new Exception("Synchronization context is required!");
        }

        public void Invoke(T argument)
        {
            _context.Post(ExecActionInContext, argument);
        }

        private void ExecActionInContext(object state)
        {
            _callbackAction((T)state);
        }
    }
}
