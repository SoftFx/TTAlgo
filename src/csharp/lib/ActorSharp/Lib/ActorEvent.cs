﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp.Lib
{
    public abstract class ActorEventBase : ActorPart, IAwaitable, IAwaiter
    {
        private List<Exception> _exceptions = new List<Exception>();
        private int respCount;
        private int fireCount;
        private bool isCompleted = true;
        private Action _callback;

        protected abstract int HandlersCount { get; }

        protected void BeginFire()
        {
            #region DEBUG
            if (!isCompleted)
                throw new InvalidOperationException("Event is busy with another async operation!");
            #endregion

            _exceptions.Clear();
            fireCount = HandlersCount;
            respCount = fireCount;
            isCompleted = false;
        }

        internal void OnRepsonce(EventResp data)
        {
            if (data.Error != null)
                _exceptions.Add(data.Error);
            respCount++;
            if (fireCount <= respCount )
            {
                isCompleted = true;
                respCount = 0;
                fireCount = 0;

                if (_callback != null)
                {
                    var toCall = _callback;
                    _callback = null;
                    toCall();
                }
            }
        }

        #region IAwaitable

        bool IAwaiter.IsCompleted => isCompleted;

        IAwaiter IAwaitable.GetAwaiter()
        {
            return this;
        }

        void IAwaiter.GetResult()
        {
            #region DEBUG
            if (!isCompleted)
                throw new InvalidOperationException("");
            #endregion

            if (_exceptions.Count > 0)
            {
                var aggEx = new AggregateException(_exceptions);
                _exceptions.Clear();
                throw aggEx;
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            #region DEBUG
            if (_callback != null)
                throw new InvalidOperationException("");
            #endregion

            _callback = continuation;
        }

        #endregion
    }

    public class ActorEvent : ActorEventBase
    {
        private HashSet<Ref<ActorCallback>> _handlers = new HashSet<Ref<ActorCallback>>();

        protected override int HandlersCount => _handlers.Count;

        protected override void ActorInit()
        {
            Ref = this.GetRef();
        }

        public Ref<ActorEvent> Ref { get; private set; }

        public void Add(ActorCallback listener)
        {
            _handlers.Add(listener.Ref);
        }

        public void Remove(ActorCallback listener)
        {
            _handlers.Remove(listener.Ref);
        }

        public void FireAndForget()
        {
            var msg = new FireEventMessage(Ref, false);

            foreach (var handler in _handlers)
                handler.PostMessage(msg);
        }

        public IAwaitable Invoke()
        {
            BeginFire();

            if (HandlersCount > 0)
            {
                var msg = new FireEventMessage(Ref, true);

                foreach (var handler in _handlers)
                    handler.PostMessage(msg);
            }

            return this;
        }

        protected override void ProcessMessage(object message)
        {
            var data = (EventResp)message;
            OnRepsonce(data);
        }
    }

    public class ActorEvent<TArgs> : ActorEventBase
    {
        private HashSet<Ref<ActorCallback<TArgs>>> _handlers = new HashSet<Ref<ActorCallback<TArgs>>>();

        protected override int HandlersCount => _handlers.Count;

        protected override void ActorInit()
        {
            Ref = this.GetRef();
        }

        public Ref<ActorEvent<TArgs>> Ref { get; private set; }

        public void Add(ActorCallback<TArgs> listener)
        {
            _handlers.Add(listener.Ref);
        }

        public void Remove(ActorCallback<TArgs> listener)
        {
            _handlers.Remove(listener.Ref);
        }

        public void FireAndForget(TArgs args)
        {
            var msg = new FireEventMessage<TArgs>(args, Ref, false);

            foreach (var handler in _handlers)
                handler.PostMessage(msg);
        }

        public IAwaitable Invoke(TArgs args)
        {
            BeginFire();

            if (HandlersCount > 0)
            {
                var msg = new FireEventMessage<TArgs>(args, Ref, true);

                foreach (var handler in _handlers)
                    handler.PostMessage(msg);
            }

            return this;
        }

        protected override void ProcessMessage(object message)
        {
            var data = (EventResp)message;
            OnRepsonce(data);
        }
    }

}
