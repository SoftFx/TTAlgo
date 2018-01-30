using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActorSharp
{
    internal class LocalChannelReader<T> : ActorPart, IChannelReader<T>, INotifyCompletion, IAwaiter<bool>
    {
        //public static readonly object RxAcknowledge = new object();

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
            #region DEBUG
            if (_rxPage != null)
                throw new Exception("Channel synchronization failure!");
            #endregion

            _rxPage = (LocalPage<T>)message;

            //System.Diagnostics.Debug.WriteLine("Rx " + _rxPage.Count);

            if (_rxPage.Count == 0)
            {
                if (_rxPage.Last)
                {
                    // confirm channel close
                    _rxPage = null;
                    _isClosed = true;
                }
                else
                {
                    // confirm read immediately
                    ReturnPage();
                    return; // no callback call
                }
            }

            if (_callback != null)
            {
                var toCall = _callback;
                _callback = null; // callback may throw exception or include further channel reads
                toCall();
            }
        }

        private void ReturnPage()
        {
            _rxPage.Clear();
            _target.PostMessage(_rxPage);
            _rxPage = null;
            _pageIndex = 0;
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
                    ReturnPage();
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
