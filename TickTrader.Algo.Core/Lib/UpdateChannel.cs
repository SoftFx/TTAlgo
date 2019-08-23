using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Core.Lib
{
    public class UpdateChannel : CrossDomainObject
    {
        private IUpdateWorker _worker;

        public void Start(bool realtime, Action<IReadOnlyList<object>> handler)
        {
            if (realtime)
                _worker = new RealtimeUpdateWorker();
            else
                _worker = new BulckUpdateWorker();

            _worker.Start(handler);
        }

        /// <summary>
        /// Can be only called by producer thread!
        /// </summary>
        public void EnqueueUpdate(object update)
        {
            _worker.EnqueueUpdate(update);
        }

        /// <summary>
        /// Can be only called by producer thread!
        /// </summary>
        public void Close()
        {
            _worker.CompleteWrite();
        }

        private interface IUpdateWorker
        {
            void Start(Action<IReadOnlyList<object>> handler);
            void CompleteWrite();
            void EnqueueUpdate(object update);
        }

        private class BulckUpdateWorker : CrossDomainObject, IUpdateWorker
        {
            private PagedGate<object> _gate = new PagedGate<object>(300);
            private Task _gatePushTask;
            private Action<IReadOnlyList<object>> _handler;

            public void Start(Action<IReadOnlyList<object>> handler)
            {
                _handler = handler;
                _gatePushTask = Task.Factory.StartNew(PushUpdates);
            }

            public void CompleteWrite()
            {
                //_gate.Close();
                _gate.Complete(); // TO DO: It will work only in backtester! Need to refactor to use in executor.
                _gatePushTask.Wait();
            }

            public void EnqueueUpdate(object update)
            {
                _gate.Write(update);
            }

            private void PushUpdates()
            {
                foreach (var page in _gate.PagedRead())
                    _handler(page);
            }
        }

        private class RealtimeUpdateWorker : CrossDomainObject, IUpdateWorker
        {
            private Action<IReadOnlyList<object>> _handler;
            private BufferBlock<object> _updateBuffer;
            private ActionBlock<object[]> _updateSender;
            private Task _batchJob;

            public void Start(Action<IReadOnlyList<object>> handler)
            {
                _handler = handler;

                var bufferOptions = new DataflowBlockOptions() { BoundedCapacity = 200 };
                var senderOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 10, SingleProducerConstrained = true };

                _updateBuffer = new BufferBlock<object>(bufferOptions);
                _updateSender = new ActionBlock<object[]>(msgList => _handler(msgList), senderOptions);

                _batchJob = _updateBuffer.BatchLinkTo(_updateSender, 50);
            }

            public void CompleteWrite()
            {
                _updateBuffer.Complete();
                _updateBuffer.Completion.Wait();
                _batchJob.Wait();
                _updateSender.Complete();
                _updateSender.Completion.Wait();
            }

            public void EnqueueUpdate(object update)
            {
                _updateBuffer.SendAsync(update).Wait();
            }
        }
    }
}
