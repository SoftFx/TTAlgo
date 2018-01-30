using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ActorSharp
{
    public class BlockingChannel<T>
    {
        private object _writeLock = new object();
        private Reader _reader;
        private LocalPage<T> _writePage1 = new LocalPage<T>();
        private LocalPage<T> _writePage2 = new LocalPage<T>();
        private int maxPageSize;
        private bool _isClosed;
        private bool _isCloseCompleted;

        internal BlockingChannel(Channel<T> channel)
        {
            if (SynchronizationContext.Current == null)
                throw new Exception("No synchroniztion context! Blocking channel should be constructed under actor context!");

            if (channel.Dicrection != ChannelDirections.In)
                throw new Exception("Only 'In' channels are supported by now!");

            maxPageSize = channel.MaxPageSize;
            _reader = new Reader(this);
            channel.Init(_reader);
        }

        public bool Write(T item)
        {
            lock (_writeLock)
            {
                while (_writePage1.Count >= maxPageSize && !_isClosed)
                    Monitor.Wait(_writeLock);

                if (_isClosed)
                    return false;

                _writePage1.Add(item);

                if (_writePage2.Count == 0) // reader is empty
                    SendNextPage();

                return true;
            }
        }

        public bool Read(out T item)
        {
            throw new NotImplementedException();
        }

        public void Close(bool clearQueue = false)
        {
            lock (_writeLock)
            {
                _isClosed = true;

                if (clearQueue)
                    _writePage1.Clear();

                if (_writePage1.Count == 0 && _writePage2.Count == 0)
                    SendNextPage(); // send empty page 

                while (!_isCloseCompleted)
                    Monitor.Wait(_writeLock);
            }
        }

        private void OnReaderClosed()
        {
            lock (_writeLock)
            {
                _writePage2.Clear();
                _isCloseCompleted = true;
                Monitor.PulseAll(_writeLock);
            }
        }

        private void OnPageRead()
        {
            lock (_writeLock)
            {
                _writePage2.Clear();

                if (_writePage1.Count > 0 || _isClosed)
                    SendNextPage();
            }
        }

        private void SendNextPage()
        {
            var pageToSend = _writePage1;
            _writePage1 = _writePage2;
            _writePage2 = pageToSend;

            pageToSend.Last = _isClosed;

            _reader.PostMessage(pageToSend);
            Monitor.PulseAll(_writeLock);
        }

        private class Reader : ActorPart, IChannelReader<T>, IAwaiter<bool>
        {
            private LocalPage<T> _rxPage;
            private int _pageIndex;
            private bool _isClosed;
            private Action _callback;
            private BlockingChannel<T> _src;

            public Reader(BlockingChannel<T> src)
            {
                _src = src;
            }

            public T Current { get; private set; }

            public bool IsCompleted => _rxPage != null || _isClosed;

            public IAwaiter<bool> GetAwaiter()
            {
                return this;
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
                        _src.OnReaderClosed();
                    }
                    else
                    {
                        _rxPage = null;
                        _pageIndex = 0;
                        _src.OnPageRead();
                    }
                }
                return true;
            }

            public void OnCompleted(Action continuation)
            {
                _callback = continuation;
            }

            protected override void ProcessMessage(object message)
            {
                if (_rxPage != null)
                    throw new Exception("Channel synchronization failure!");

                _rxPage = (LocalPage<T>)message;
                if (_rxPage.Count == 0 && _rxPage.Last)
                {
                    _rxPage = null;
                    _isClosed = true;
                    _src.OnReaderClosed();
                }

                if (_callback != null)
                {
                    var toCall = _callback;
                    _callback = null; // callback may throw exception
                    toCall();
                }
            }
        }
    }
}
