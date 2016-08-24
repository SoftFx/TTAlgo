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
        internal InvokeStartegy()
        {
        }

        public void Init(PluginBuilder builder)
        {
            this.Builder = builder;
        }

        protected PluginBuilder Builder { get; private set; }

        public abstract void Start();
        public abstract Task Stop();
        public abstract void Abort();
        //public abstract Task OnUpdate(FeedUpdate[] updates);
        //public abstract Task OnUpdate(OrderExecReport report);
        public abstract void EnqueueInvoke(Action<PluginBuilder> a);
        public abstract void EnqueueInvoke(Task t);
    }

    [Serializable]
    public class DataflowInvokeStartegy : InvokeStartegy
    {
        private ActionBlock<object> taskQueue;
        private CancellationTokenSource cancelSrc;

        //public override Task OnUpdate(FeedUpdate[] updates)
        //{
        //    return taskQueue.SendAsync(updates);
        //}

        public override void Start()
        {
            this.cancelSrc = new CancellationTokenSource();
            var options = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = cancelSrc.Token };
            taskQueue = new ActionBlock<object>((Action<object>)Process, options);
            //Enqueue(() => context.Builder.InvokeInit());
            // TO DO : split into reasonably sized chunks
            //Enqueue(() => BatchBuild(loadedPoints));
            //Enqueue(() => context.Builder.InvokeOnStart());
        }

        public async override Task Stop()
        {
            try
            {
                taskQueue.Complete();
                await taskQueue.Completion;
            }
            catch (OperationCanceledException) { }

            //await Task.Factory.StartNew(() => context.Builder.InvokeOnStop());
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
            if (data is Action<PluginBuilder>)
                ((Action<PluginBuilder>)data)(Builder);
            if (data is Task)
                ((Task)data).RunSynchronously();
            //else if (data is FeedUpdate[])
            //{
            //    foreach (var update in (FeedUpdate[])data)
            //        UpdateBuild(update);
            //}
        }

        //private void UpdateBuild(FeedUpdate update)
        //{
        //    var result = context.UpdateBuffers(update);
        //    if (result == BufferUpdateResults.Extended)
        //    {
        //        context.Builder.IncreaseVirtualPosition();
        //        context.Builder.InvokeCalculate(false);
        //        context.InvokeFeedEvents(update);
        //    }
        //    else if (result == BufferUpdateResults.LastItemUpdated)
        //    {
        //        context.Builder.InvokeCalculate(true);
        //        context.InvokeFeedEvents(update);
        //    }

        //    context.Builder.InvokeOnQuote(update.Quote);
        //}

        //private void BatchBuild(int x)
        //{
        //    context.Builder.StartBatch();

        //    for (int i = 0; i < x; i++)
        //    {
        //        context.Builder.IncreaseVirtualPosition();
        //        context.Builder.InvokeCalculate(false);
        //    }

        //    context.Builder.StopBatch();
        //}

    }
}
