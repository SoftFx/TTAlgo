using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace ActorSharp
{
    public class BlockingChannel<T>
    {
        private object _writeLock = new object();
        private object _readLock = new object();
        private Reader _reader;
        private Writer _writer;
        private LocalPage<T> _writePage1 = new LocalPage<T>();
        private LocalPage<T> _writePage2 = new LocalPage<T>();
        private LocalPage<T> _readPage;
        private int _readIndex;
        private int maxPageSize;
        private bool _isClosed;
        private bool _isCloseCompleted;
        private ExceptionDispatchInfo _writerError;

        internal BlockingChannel(Channel<T> channel)
        {
            if (SynchronizationContext.Current == null)
                throw new Exception("No synchroniztion context! Blocking channel should be constructed under actor context!");

            if (channel.Dicrection == ChannelDirections.Duplex)
                throw new Exception("Duplex channels are not supported by now!");

            maxPageSize = channel.MaxPageSize;
            if (channel.Dicrection == ChannelDirections.In)
            {
                _reader = new Reader(this);
                channel.Init(_reader);
            }
            else
            {
                _writer = new Writer(this, channel.MaxPageSize);
                channel.Init(_writer);
            }
        }

        public bool Write(T item)
        {
            lock (_writeLock)
            {
                WaitWrite();

                if (_isClosed)
                    return false;

                _writePage1.Add(item);

                TrySend();

                return true;
            }
        }

        public bool WriteAll(IEnumerable<T> items)
        {
            var e = items.GetEnumerator();

            while (true)
            {
                lock (_writeLock)
                {
                    WaitWrite();

                    if (_isClosed)
                        return false;

                    while (_writePage1.Count < maxPageSize)
                    {
                        if (!e.MoveNext())
                            return true;
                        _writePage1.Add(e.Current);
                    }

                    TrySend();
                }
            }
        }

        public bool Read(out T item)
        {
            lock (_readLock)
            {
                WaitRead();

                if (_readPage == null && _isClosed)
                {
                    item = default(T);
                    return false;
                }

                item = _readPage[_readIndex++];

                if (_readIndex == _readPage.Count)
                    ReturnPage();

                return !_isClosed;
            }
        }

        public T[] ReadPage()
        {
            lock (_readLock)
            {
                WaitRead();

                if (_readPage == null && _isClosed)
                    return null;

                var resultSize = _readPage.Count - _readIndex;
                var result = new T[resultSize];
                _readPage.CopyTo(_readIndex, result, 0, resultSize);

                ReturnPage();

                return result;
            }
        }

        public void ClearQueue()
        {
            lock (_writeLock) _writePage1.Clear();
        }

        public void Close(Exception ex = null)
        {
            CloseWriter(ex);
            ClosedReader(ex);
        }

        #region Writer interop

        private void OnPageWrite(LocalPage<T> page)
        {
            lock (_readLock)
            {
                if (_readPage != null)
                    throw new Exception("Channel synchronization failure!");

                _readPage = page;
                _isClosed = page.Last;
                if (_isClosed && _readPage.Count == 0)
                    _readPage = null;
                _writerError = page.Error;
                Monitor.PulseAll(_readLock);
            }
        }

        private void WaitRead()
        {
            while (_readPage == null && !_isClosed)
                Monitor.Wait(_readLock);
        }

        private void ReturnPage()
        {
            _readPage.Clear();
            _writer.PostMessage(_readPage);
            _readPage = null;
            _readIndex = 0;
        }

        private void CloseWriter(Exception ex)
        {
            if (_writer == null)
                return;

            lock (_readLock)
            {
                if (_isClosed)
                {
                    _isClosed = true;

                    _readPage?.Clear();
                    _readPage = null;
                    _readIndex = 0;

                    var closeReq = new CloseWriterRequest(ex);
                    _writer.PostMessage(closeReq);

                    Monitor.PulseAll(_readLock);

                    return;
                }
            }
        }

        #endregion

        #region Reader interop

        private void WaitWrite()
        {
            while (_writePage1.Count >= maxPageSize && !_isClosed)
                Monitor.Wait(_writeLock);
        }

        private void TrySend()
        {
            if (_writePage2.Count == 0) // reader is empty
                SendNextPage();
        }

        private void OnReaderClosed()
        {
            lock (_writeLock)
            {
                _isClosed = true;

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

        private void ClosedReader(Exception ex)
        {
            if (_reader == null)
                return;

            lock (_writeLock)
            {
                if (_isClosed)
                {
                    while (!_isCloseCompleted)
                        Monitor.Wait(_writeLock);
                    return;
                }

                _isClosed = true;
                if (ex != null)
                    _writePage1.Error = ExceptionDispatchInfo.Capture(ex);

                if (_writePage1.Count == 0 && _writePage2.Count == 0)
                    SendNextPage(); // send empty page 

                while (!_isCloseCompleted)
                    Monitor.Wait(_writeLock);
            }
        }

        #endregion

        private class Reader : ActorPart, IChannelReader<T>, IAwaiter<bool>
        {
            private LocalPage<T> _rxPage;
            private int _pageIndex;
            private bool _isClosed;
            private Action _callback;
            private BlockingChannel<T> _src;
            private ExceptionDispatchInfo _fault;

            public Reader(BlockingChannel<T> src)
            {
                _src = src;
            }

            public T Current { get; private set; }

            public bool IsCompleted => _rxPage != null || _isClosed;

            public void Close(Exception ex)
            {
                _src.OnReaderClosed();
            }

            public IAwaiter<bool> GetAwaiter()
            {
                return this;
            }

            public bool GetResult()
            {
                if (_isClosed)
                {
                    if (_fault != null)
                        _fault.Throw();
                    return false;
                }

                Current = _rxPage[_pageIndex++];
                if (_pageIndex == _rxPage.Count)
                {
                    if (_rxPage.Last)
                    {
                        _fault = _rxPage.Error;
                        _isClosed = true;
                        _rxPage = null;
                        _pageIndex = 0;
                        _src.OnReaderClosed();
                        if (_fault != null)
                            _fault.Throw();
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
                    _fault = _rxPage.Error;
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

        private class Writer : ActorPart, IChannelWriter<T>, IAwaitable<bool>, IAwaiter<bool>, IAwaitable, IAwaiter
        {
            private BlockingChannel<T> _src;
            private LocalPage<T> _queuePage = new LocalPage<T>();
            private LocalPage<T> _cachedPage = new LocalPage<T>();
            private bool _isClosed;
            private Action _callback;
            private int _confirmationCounter;
            private int _maxPageSize;

            private bool IsChannelFree => _cachedPage != null;

            public Writer(BlockingChannel<T> src, int pageSize)
            {
                _src = src;
                _maxPageSize = pageSize;
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

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public IAwaitable Close(Exception ex = null)
            {
                CheckSync();

                if (!_isClosed)
                {
                    _isClosed = true;
                    _queuePage.Last = true;
                    if (ex != null)
                        _queuePage.Error = ExceptionDispatchInfo.Capture(ex);
                    _confirmationCounter = 1;
                    TrySendPage();
                }

                return this;
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

            private void CheckSync()
            {
                #region DEBUG
                if (_callback != null || _confirmationCounter > 0)
                    throw new InvalidOperationException("Channel is busy with another async operation!");
                #endregion
            }

            private void TrySendPage()
            {
                if (IsChannelFree)
                    SendPage();
            }

            private void SendPage()
            {
                _src.OnPageWrite(_queuePage);

                _queuePage = _cachedPage;
                _cachedPage = null;

                if (_confirmationCounter == 0 || --_confirmationCounter == 0)
                {
                    var toInvoke = _callback;
                    _callback = null;
                    toInvoke?.Invoke();
                }
            }

            #region IAwaitable<bool>

            public bool IsCompleted => _confirmationCounter == 0 && _queuePage.Count < _maxPageSize; // read

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

            public IAwaiter<bool> GetAwaiter()
            {
                return this;
            }

            public bool GetResult()
            {
                return !_isClosed;
            }

            public void OnCompleted(Action continuation)
            {
                _callback = continuation;
            }

            #endregion

            #region IAwaitable

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
}
