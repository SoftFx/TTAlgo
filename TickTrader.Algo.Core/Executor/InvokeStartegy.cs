using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Core
{
    public abstract class InvokeStartegy
    {
        internal InvokeStartegy()
        {
        }

        public abstract void Start(IInvokeStrategyContext context, int loadedPositions);
        public abstract Task Stop(bool cancelPendingUpdates);
        public abstract Task OnUpdate(FeedUpdate[] updates);
        public abstract void InvokeOnPluginThread(Action a);
    }

    public class DataflowInvokeStartegy : InvokeStartegy
    {
        private IInvokeStrategyContext context;
        private ActionBlock<object> taskQueue;
        private CancellationTokenSource cancelSrc;

        public override void InvokeOnPluginThread(Action a)
        {
        }

        public override Task OnUpdate(FeedUpdate[] updates)
        {
            return taskQueue.SendAsync(updates);
        }

        public override void Start(IInvokeStrategyContext context, int loadedPositions)
        {
            this.context = context;
            this.cancelSrc = new CancellationTokenSource();
            var options = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = cancelSrc.Token };
            taskQueue = new ActionBlock<object>((Action<object>)Process, options);
            Enqueue(() => context.Builder.InvokeOnStart());
            Enqueue(() => context.Builder.InvokeInit());
            // TO DO : split into reasonably sized chunks
            Enqueue(() => BatchBuild(loadedPositions));
        }

        private void Enqueue(Action a)
        {
            taskQueue.Post(a);
        }

        private void Process(object data)
        {
            if (data is Action)
                ((Action)data)();
            else if (data is FeedUpdate[])
            {
                foreach (var update in (FeedUpdate[])data)
                    UpdateBuild(update);
            }
        }

        private void UpdateBuild(FeedUpdate update)
        {
            var result = context.UpdateBuffers(update);
            if (result == BufferUpdateResults.Extended)
            {
                context.Builder.IncreaseVirtualPosition();
                context.Builder.InvokeCalculate(false);
                context.InvokeFeedEvents(update);
            }
            else if (result == BufferUpdateResults.LastItemUpdated)
            {
                context.Builder.InvokeCalculate(true);
                context.InvokeFeedEvents(update);
            }
        }

        private void BatchBuild(int x)
        {
            context.Builder.StartBatch();

            for (int i = 0; i < x; i++)
            {
                context.Builder.IncreaseVirtualPosition();
                context.Builder.InvokeCalculate(false);
            }

            context.Builder.StopBatch();
        }

        public async override Task Stop(bool cancelPendingUpdates)
        {
            try
            {
                if (cancelPendingUpdates)
                    cancelSrc.Cancel();
                else
                    taskQueue.Complete();
                await taskQueue.Completion;
            }
            catch (OperationCanceledException) { }
        }
    }
}
