using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    /// <summary>
    /// This queue is highly optimized but have a lot of restirctions! It's restricted to one producer thread and one consumer thread.
    /// This queue is not fully thread safe: Methods Write() and Complete() can only be called by producer thread!
    /// Methods PagedRead() and GetEnumerator() can only be called by consumer thread!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedGate<T> : IEnumerable<T>
    {
        private readonly Queue<List<T>> _sendQueue = new Queue<List<T>>();
        private readonly Queue<List<T>> _freePages = new Queue<List<T>>();
        private List<T> _writeBuff;
        private List<T> _readBuff;
        private readonly int _queueMaxLen;
        private readonly int _maxBuffLen;
        private bool _writeCompleted;

        public PagedGate(int pageSize, int maxQueueSize = 1)
        {
            if (pageSize < 1)
                throw new ArgumentException("pageSize");

            if (maxQueueSize < 1)
                throw new ArgumentException("pageSize");

            _maxBuffLen = pageSize;
            _queueMaxLen = maxQueueSize;

            _writeBuff = new List<T>(pageSize);

            for (int i = 0; i <= maxQueueSize; i++)
                _freePages.Enqueue(new List<T>(pageSize));
        }

        /// <summary>
        /// Not thread safe! Can only be called by producer thread!
        /// </summary>
        public void Write(T item)
        {
            //if (_closed) // not 100% guarantee because of no synchronization
            //    return false;

            _writeBuff.Add(item);

            if (_writeBuff.Count >= _maxBuffLen)
                WritePage(false);
        }

        /// <summary>
        /// Not thread safe! Can only be called by producer thread!
        /// </summary>
        public void Complete()
        {
            if (_writeBuff.Count > 0)
                WritePage(true);
            else
            {
                lock (_sendQueue)
                {
                    _writeCompleted = true;
                    Monitor.Pulse(_sendQueue);
                }
            }
        }

        private void WritePage(bool final)
        {
            lock (_sendQueue)
            {
                if (_writeCompleted)
                    return;

                while (_sendQueue.Count >= _queueMaxLen)
                    Monitor.Wait(_sendQueue);

                _sendQueue.Enqueue(_writeBuff);
                _writeBuff = _freePages.Dequeue();
                Monitor.Pulse(_sendQueue);

                if (final)
                    _writeCompleted = true;
            }
        }

        private bool ReadPage()
        {
            lock (_sendQueue)
            {
                if (_readBuff != null)
                {
                    _readBuff.Clear();
                    _freePages.Enqueue(_readBuff);
                    _readBuff = null;
                }

                while (_sendQueue.Count == 0)
                {
                    if (_writeCompleted && _sendQueue.Count == 0)
                        return false;

                    Monitor.Wait(_sendQueue);
                }

                _readBuff = _sendQueue.Dequeue();
                Monitor.Pulse(_sendQueue);
                return true;
            }
        }

        /// <summary>
        /// Not thread safe! Can only be called by consumer thread!
        /// Lists are re-used, so please do not cache them! 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<List<T>> PagedRead()
        {
            while (ReadPage())
                yield return _readBuff;
        }

        /// <summary>
        /// Not thread safe! Can only be called by consumer thread!
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            while (ReadPage())
            {
                for (int i = 0; i < _readBuff.Count; i++)
                    yield return _readBuff[i];
            }
        }

        /// <summary>
        /// Not thread safe! Can only be called by consumer thread!
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
