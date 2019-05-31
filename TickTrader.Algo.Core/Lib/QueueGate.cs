using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class QueueGate<T> : IEnumerable<T>
    {
        private object _lockObj = new object();
        private List<T> _writeBuff;
        private List<T> _readBuff;
        private int _buffSize;
        private Queue<List<T>> _queue = new Queue<List<T>>();
        private Queue<List<T>> _emptyBuffCache = new Queue<List<T>>();
        //private bool _readRequest;
        //private bool _writeRequest;
        private bool _closed;

        public QueueGate(int buffSize, int queueLen)
        {
            _buffSize = buffSize;
            _writeBuff = new List<T>(buffSize);
            //_readBuff = new List<T>(buffSize);

            for (int i = 0; i < queueLen; i++)
                _emptyBuffCache.Enqueue(new List<T>(buffSize));
        }

        public bool Write(T item)
        {
            if (_closed) // not 100% guarantee because of no synchronization
                return false;

            _writeBuff.Add(item);

            if (_writeBuff.Count >= _buffSize)
                return WritePage();

            return true;
        }

        public void CompleteWrite()
        {
            if (_writeBuff.Count > 0)
                WritePage();
            Close();
        }

        public void Close()
        {
            lock (_lockObj)
            {
                //_readRequest = false;
                //_writeRequest = false;
                _closed = true;
                Monitor.PulseAll(_lockObj);
            }
        }

        private bool WritePage()
        {
            lock (_lockObj)
            {
                if (_closed)
                    return false;

                _queue.Enqueue(_writeBuff);
                Monitor.Pulse(_lockObj);
                _writeBuff = null;

                while (_emptyBuffCache.Count == 0)
                {
                    Monitor.Wait(_lockObj);
                    if (_closed)
                        return false;
                }

                _writeBuff = _emptyBuffCache.Dequeue();
                return true;
            }
        }

        private bool ReadPage()
        {
            lock (_lockObj)
            {
                if (_closed)
                    return false;

                if (_readBuff != null)
                {
                    _readBuff.Clear();
                    _emptyBuffCache.Enqueue(_readBuff);
                    Monitor.Pulse(_lockObj);
                }

                while (_queue.Count == 0)
                {
                    Monitor.Wait(_lockObj);
                    if (_closed)
                        return false;
                }

                _readBuff = _queue.Dequeue();
                return true;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (ReadPage())
            {
                foreach (T item in _readBuff)
                    yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
