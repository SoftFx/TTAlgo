using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActorSharp
{
    internal class LocalTxChannel<T> : ActorPart, ITxChannel<T>, IAwaiter<bool>, IAwaitable<bool>, IAwaitable, IAwaiter
    {
        private ActorPart _target;
        private LocalPage<T> _waitPage = new LocalPage<T>();
        private LocalPage<T> _sentPage = new LocalPage<T>();
        private int _maxPageSize;
        private Action _callback;
        private bool _isClosed;

        public void Init(ActorPart target, int pageSize)
        {
            _target = target;
            _maxPageSize = pageSize;
        }

        public IAwaitable Close()
        {
            CheckCallback();

            if (!_isClosed)
            {
                _isClosed = true;
                _waitPage.Last = true;
                TrySendPage();
            }

            return this;
        }

        public IAwaitable<bool> TryWrite(T item)
        {
            return Write(item);
        }

        public IAwaitable<bool> Write(T item)
        {
            CheckCallback();
            CheckClosed();

            _waitPage.Add(item);
            TrySendPage();
            return this;
        }

        private void TrySendPage()
        {
            if (_sentPage.Count == 0) // ready to send (no items are on other side)
                SendPage();
        }

        private void CheckCallback()
        {
            if (_callback != null)
                throw new InvalidOperationException("Channel is busy with another async operation!");
        }

        private void CheckClosed()
        {
            if (_isClosed)
                throw new InvalidOperationException("Channel is closed!");
        }

        private void SendPage()
        {
            var toSend = _waitPage;
            _waitPage = _sentPage;
            _sentPage = toSend;

            _target.PostMessage(toSend);

            var toInvoke = _callback;
            _callback = null;
            toInvoke?.Invoke();
        }

        protected override void ProcessMessage(object message)
        {
            if (message == LocalRxChannel<T>.RxAcknowledge)
            {
                _sentPage.Clear();
                if (_waitPage.Count > 0)
                    SendPage();
            }
            else
                throw new Exception("Unsupported message!");
        }

        #region IAwaitable

        bool IAwaiter<bool>.IsCompleted => _waitPage.Count < _maxPageSize; // read
        bool IAwaiter.IsCompleted => _waitPage.Count == 0; // close

        bool IAwaiter<bool>.GetResult()
        {
            return true;
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            _callback = continuation;
        }

        IAwaiter<bool> IAwaitable<bool>.GetAwaiter()
        {
            return this;
        }

        IAwaiter IAwaitable.GetAwaiter()
        {
            return this;
        }

        void IAwaiter.GetResult()
        {
        }

        #endregion
    }
}
