using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core.DataflowConcept
{
    public abstract class ExecutionContext
    {
        public abstract T Activate<T>() where T : MarshalByRefObject, new();
        public abstract void InvokeAction<TData, TRef>(TData data, TRef target, Action<TData, TRef> action);

        [Serializable]
        public class GenericDelegate<TData, TRef> : IOneWayDelegate
        {
            private TData data;
            private TRef refObj;
            private Action<TData, TRef> action;

            public GenericDelegate(TData data, TRef refObj, Action<TData, TRef> action)
            {
                this.data = data;
                this.refObj = refObj;
                this.action = action;
            }

            public void Invoke()
            {
                action(data, refObj);
            }
        }
    }

    public interface IOneWayDelegate
    {
        void Invoke();
    }

    public class BypassContext : ExecutionContext
    {
        public override T Activate<T>()
        {
            return new T();
        }

        public override void InvokeAction<TData, TRef>(TData data, TRef target, Action<TData, TRef> action)
        {
            action(data, target);
        }
    }

    public class MsgQueueContext : ExecutionContext
    {
        private ActionBlock<IOneWayDelegate> actror;

        public MsgQueueContext()
        {
            actror = new ActionBlock<IOneWayDelegate>(b => b.Invoke());
        }

        public override T Activate<T>()
        {
            return new T();
        }

        public override void InvokeAction<TData, TRef>(TData data, TRef target, Action<TData, TRef> action)
        {
            actror.Post(new GenericDelegate<TData, TRef>(data, target, action));
        }
    }

    public class IsolatedMsgQueueContext : ExecutionContext
    {
        private IsolatedAgent isolated;
        private BufferBlock<IOneWayDelegate> inputBuffer;
        private ActionBlock<IOneWayDelegate[]> marshalBlock;

        internal IsolatedMsgQueueContext(AlgoSandbox sandbox)
        {
            isolated = sandbox.Activate<IsolatedAgent>();
            inputBuffer = new BufferBlock<IOneWayDelegate>();
            marshalBlock = new ActionBlock<IOneWayDelegate[]>(b => isolated.Enqueue(b));
        }

        public override T Activate<T>()
        {
            return isolated.Activate<T>();
        }

        public override void InvokeAction<TData, TRef>(TData data, TRef target, Action<TData, TRef> action)
        {
            inputBuffer.Post(new GenericDelegate<TData, TRef>(data, target, action));
        }

        public class IsolatedAgent : MarshalByRefObject
        {
            public T Activate<T>() where T : MarshalByRefObject, new()
            {
                return new T();
            }

            public void Enqueue(IOneWayDelegate[] batch)
            {
                foreach (var msg in batch)
                    msg.Invoke();
            }
        }
    }
}
