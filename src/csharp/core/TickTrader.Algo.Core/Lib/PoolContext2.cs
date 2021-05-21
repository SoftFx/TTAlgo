using System;
using System.Threading;
using System.Threading.Channels;

namespace TickTrader.Algo.Core.Lib
{
    public class PoolContext2 : SynchronizationContext
    {
        private Channel<CallbackItem> _channel;
        private int _maxPageSize = 10;


        public PoolContext2()
        {
            _channel = Channel.CreateUnbounded<CallbackItem>(new UnboundedChannelOptions { SingleReader = true });
            ThreadPool.QueueUserWorkItem(ProcessLoop);
            //Task.Factory.StartNew(ProcessLoop);
        }


        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotImplementedException("Should not Send() in Actor famework!");
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            Enqueue(new CallbackItem(d, state));
        }

        private void Enqueue(CallbackItem item)
        {
            if (!_channel.Writer.TryWrite(item))
                throw new Exception("Can't enqueue item!");
        }

        private async void ProcessLoop(object state)
        {
            if (await _channel.Reader.WaitToReadAsync())
            {
                ProcessItems(_channel);
                ThreadPool.QueueUserWorkItem(ProcessLoop);
                //Task.Factory.StartNew(ProcessLoop);
            }
        }

        private void ProcessItems(ChannelReader<CallbackItem> reader)
        {
            try
            {
                SetSynchronizationContext(this);

                for (var i = 0; i < _maxPageSize && reader.TryRead(out var item); i++)
                    item.Callback(item.State);

            }
            catch (Exception ex)
            {
                Environment.FailFast("Uncaught exception in actor: " + ex.Message, ex);
                throw;
            }
            finally
            {
                SetSynchronizationContext(null);
            }
        }


        private struct CallbackItem
        {
            public CallbackItem(SendOrPostCallback callback, object state)
            {
                Callback = callback;
                State = state;
            }

            public SendOrPostCallback Callback { get; private set; }
            public object State { get; private set; }
        }
    }
}
