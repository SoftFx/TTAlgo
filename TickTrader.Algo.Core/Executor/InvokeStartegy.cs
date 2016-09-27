using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class InvokeStartegy
    {
        private Action<ExecutorException> onCoreError;
        private Action<Exception> onRuntimeError;

        internal InvokeStartegy()
        {
        }

        public void Init(PluginBuilder builder, Action<ExecutorException> onCoreError, Action<Exception> onRuntimeError)
        {
            this.Builder = builder;
            this.onCoreError = onCoreError;
            this.onRuntimeError = onRuntimeError;
        }

        protected PluginBuilder Builder { get; private set; }

        public abstract void Start();
        public abstract Task Stop();
        public abstract void Abort();
        public abstract void EnqueueInvoke(Action<PluginBuilder> a);
        public abstract void EnqueueInvoke(Task t);

        protected void OnError(ExecutorException ex)
        {
            onCoreError?.Invoke(ex);
        }

        protected void OnRuntimeException(Exception ex)
        {
            onRuntimeError?.Invoke(ex);
        }
    }

    [Serializable]
    public class DataflowInvokeStartegy : InvokeStartegy
    {
        private ActionBlock<object> taskQueue;
        private CancellationTokenSource cancelSrc;

        public override void Start()
        {
            this.cancelSrc = new CancellationTokenSource();
            var options = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = cancelSrc.Token };
            taskQueue = new ActionBlock<object>((Action<object>)Process, options);
        }

        public async override Task Stop()
        {
            try
            {
                taskQueue.Complete();
                await taskQueue.Completion;
            }
            catch (OperationCanceledException) { }
        }

        public override void Abort()
        {
            cancelSrc.Cancel();
        }

        public override void EnqueueInvoke(Action<PluginBuilder> a)
        {
            taskQueue.Post(a);
        }

        public override void EnqueueInvoke(Task t)
        {
            taskQueue.Post(t);   
        }

        private void Enqueue(Action a)
        {
            taskQueue.Post(a);
        }

        private void Process(object data)
        {
            try
            {
                if (data is Action<PluginBuilder>)
                    ((Action<PluginBuilder>)data)(Builder);
                if (data is Task)
                    ((Task)data).RunSynchronously();
            }
            catch (ExecutorException ex)
            {
                OnError(ex);
            }
            catch (Exception ex)
            {
                OnRuntimeException(ex);
            }
        }
    }
}
