using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActorSharp
{
    internal class LocalRxChannel<T> : ActorPart, IRxChannel<T>, INotifyCompletion, IAwaiter<bool>
    {
        public static readonly object RxAcknowledge = new object();

        private ActorPart _target;
        private LocalPage<T> _rxPage;
        private int _pageIndex;
        private Action _callback;
        private bool _isClosed;

        public void Init(ActorPart target)
        {
            _target = target;
        }

        public T Current { get; private set; }

        protected override void ProcessMessage(object message)
        {
            if (_rxPage != null)
                throw new Exception("Channel synchronization failure!");

            _rxPage = (LocalPage<T>)message;
            if (_rxPage.Count == 0 && _rxPage.Last)
            {
                _rxPage = null;
                _isClosed = true;
            }

            if (_callback != null)
            {
                var toCall = _callback;
                _callback = null; // callback may throw exception
                toCall();
            }
        }

        #region IAwaitable<T>

        bool IAwaiter<bool>.IsCompleted => _rxPage != null || _isClosed;

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            _callback = continuation;
        }

        public bool GetResult()
        {
            if (_isClosed)
                return false;

            Current = _rxPage[_pageIndex++];
            if (_pageIndex == _rxPage.Count)
            {
                if (_rxPage.Last)
                {
                    _isClosed = true;
                    _rxPage = null;
                    _pageIndex = 0;
                }
                else
                {
                    _rxPage = null;
                    _pageIndex = 0;
                    _target.PostMessage(RxAcknowledge);
                }
            }
            return true;
        }

        IAwaiter<bool> IAwaitable<bool>.GetAwaiter()
        {
            return this;
        }

        #endregion
    }
}
