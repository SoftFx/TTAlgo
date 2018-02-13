using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActorSharp
{
    internal class LocalChannelWriter<T> : ActorPart, IChannelWriter<T>, IAwaiter<bool>, IAwaitable<bool>, IAwaitable, IAwaiter
    {
        private ActorPart _target;
        private LocalPage<T> _queuePage = new LocalPage<T>();
        private LocalPage<T> _cachedPage = new LocalPage<T>();
        private int _maxPageSize;
        private Action _callback;
        private bool _isClosed;
        private int _confirmationCounter;

        private bool IsCahnnelFree => _cachedPage != null;

        public void Init(ActorPart target, int pageSize)
        {
            _target = target;
            _maxPageSize = pageSize;
        }

        public IAwaitable Close(Exception error = null)
        {
            CheckSync();

            if (!_isClosed)
            {
                _isClosed = true;
                _queuePage.Last = true;
                _queuePage.Error = error;
                _confirmationCounter = 1;
                TrySendPage();
            }

            return this;
        }

        public IAwaitable<bool> Write(T item)
        {
            CheckSync();

            if (!_isClosed)
            {
                _queuePage.Add(item);
                TrySendPage();
            }

            return this;
        }

        public IAwaitable<bool> ConfirmRead()
        {
            CheckSync();

            #if DEBUG
            if (_confirmationCounter > 0)
                throw new Exception("Channel is already confirming read!");
            #endif

            _confirmationCounter = 2;
            TrySendPage();
            return this;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        private void TrySendPage()
        {
            if (IsCahnnelFree)
                SendPage();
        }

        private void CheckSync()
        {
            #region DEBUG
            if (_callback != null || _confirmationCounter > 0)
                throw new InvalidOperationException("Channel is busy with another async operation!");
            #endregion
        }

        private void SendPage()
        {
            #region DEBUG
            if (_cachedPage == null)
                throw new Exception("Channel synchronization failure!");
            #endregion

            //System.Diagnostics.Debug.WriteLine("Send " + _queuePage.Count);

            _target.PostMessage(_queuePage);

            _queuePage = _cachedPage;
            _cachedPage = null;

            if (_confirmationCounter == 0 || --_confirmationCounter == 0)
            {
                var toInvoke = _callback;
                _callback = null;
                toInvoke?.Invoke();
            }
        }

        protected override void ProcessMessage(object message)
        {
            var page = message as LocalPage<T>;

            if (page != null)
            {
                if (_cachedPage != null || page.Count > 0)
                    throw new Exception("Channel synchronization failure!");

                _cachedPage = page; // page is returned

                if (_queuePage.Count > 0 || _confirmationCounter > 0)
                    SendPage();
            }
            else if (message is CloseWriterRequest)
            {
                _isClosed = true;
                _queuePage.Clear();

                var toInvoke = _callback;
                _callback = null;
                toInvoke?.Invoke();
            }
            else
                throw new Exception("Unsupported message!");
        }

        #region IAwaitable (Write operation)

        bool IAwaiter<bool>.IsCompleted => _confirmationCounter == 0 && _queuePage.Count < _maxPageSize; // read

        bool IAwaiter<bool>.GetResult()
        {
            return !_isClosed;
        }

        IAwaiter<bool> IAwaitable<bool>.GetAwaiter()
        {
            return this;
        }

        #endregion

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            _callback = continuation;
        }

        #region IAwaitable (Close operation)

        bool IAwaiter.IsCompleted => _queuePage.Count == 0; // close

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
