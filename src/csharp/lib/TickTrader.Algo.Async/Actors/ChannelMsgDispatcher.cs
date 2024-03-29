﻿using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async.Actors
{
    public class ChannelMsgDispatcherFactory : IMsgDispatcherFactory
    {
        public IMsgDispatcher CreateDispatcher(string actorName, int maxBatch) => new ChannelMsgDispatcher(actorName, maxBatch);
    }


    public class ChannelMsgDispatcher : IMsgDispatcher
    {
        private readonly string _actorName;
        private readonly int _maxBatch;
        private readonly Channel<object> _channel;
        private readonly SyncContextAdapter _syncContext;
        
        private Action<object> _msgHandler;
        private Task _msgLoopTask;
        private bool _doProcessing;


        public ChannelMsgDispatcher(string actorName, int maxBatch)
        {
            _actorName = actorName;
            _maxBatch = maxBatch;

            _channel = DefaultChannelFactory.CreateForManyToOne<object>();
            _syncContext = new SyncContextAdapter(this);
        }

        public void PostMessage(object msg)
        {
            if (!_channel.Writer.TryWrite(msg))
            {
                if (msg is CallbackMsg)
                    return; // some task continuations are invoked from thread pool and can crash application

                if (msg is IAskMsg askMsg)
                    msg = askMsg.Request;

                throw Errors.PostMessageFailed(_actorName, msg.GetType().FullName);
            }
        }

        public void Start(Action<object> msgHandler)
        {
            if (_msgLoopTask != null)
                throw Errors.MsgDispatcherAlreadyStarted(_actorName);

            _msgHandler = msgHandler;
            _doProcessing = true;
            _msgLoopTask = Task.Factory.StartNew(ProcessLoop).Unwrap();
        }

        public async Task Stop()
        {
            if (!_doProcessing)
                throw Errors.MsgDispatcherAlreadyStopped(_actorName);

            _doProcessing = false;
            _channel.Writer.Complete();
            await _msgLoopTask;
            _msgLoopTask = null;
        }


        private async Task ProcessLoop()
        {
            var reader = _channel.Reader;
            await Task.Yield();

            while (_doProcessing && await reader.WaitToReadAsync())
            {
                ProcessItems(reader, _msgHandler, _maxBatch);

                await Task.Yield(); //break sync processing
            }
        }

        private void ProcessItems(ChannelReader<object> reader, Action<object> msgHandler, int maxBatch)
        {
            try
            {
                _syncContext.Enter();

                for (var i = 0; i < maxBatch && _doProcessing && reader.TryRead(out var msg); i++)
                    msgHandler(msg);

            }
            catch (Exception ex)
            {
                ActorSystem.OnActorError(_actorName, ex);
            }
            finally
            {
                _syncContext.Exit();
            }
        }
    }
}
