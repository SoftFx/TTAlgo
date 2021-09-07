using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class FlipGate<T> : IEnumerable<T>
    {
        private object _lockObj = new object();
        private List<T> _writeBuff;
        private List<T> _readBuff;
        private int _buffSize;
        private bool _readRequest;
        private bool _writeRequest;
        private bool _closed;
        private bool _completed;

        public FlipGate(int buffSize)
        {
            _buffSize = buffSize;
            _writeBuff = new List<T>(buffSize);
            _readBuff = new List<T>(buffSize);
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

        private void Flip()
        {
            _readBuff.Clear();
            Ref.Swap(ref _readBuff, ref _writeBuff);
        }

        private bool WritePage()
        {
            lock (_lockObj)
            {
                if (_closed)
                    return false;

                if (_readRequest)
                {
                    Flip();
                    _readRequest = false;
                    Monitor.Pulse(_lockObj);
                    return true;
                }
                else
                {
                    _writeRequest = true;
                    while (_writeRequest)
                    {
                        if (_closed)
                            return false;

                        Monitor.Wait(_lockObj);
                    }
                    return true;
                }
            }
        }

        private bool ReadPage()
        {
            lock (_lockObj)
            {
                if (_completed)
                    return false;

                if (_writeRequest)
                {
                    Flip();
                    _writeRequest = false;
                    Monitor.Pulse(_lockObj);
                    if (_closed)
                        _completed = true;
                    return true;
                }
                else
                {
                    _readRequest = true;
                    while (_readRequest)
                    {
                        if (_closed)
                        {
                            _completed = true;
                            _readRequest = false;
                            return false;
                        }

                        Monitor.Wait(_lockObj);
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// Lists are re-used, so please do not cache them! 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<List<T>> PagedRead()
        {
            while (ReadPage())
                yield return _readBuff;
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (ReadPage())
            {
                for (int i = 0; i < _readBuff.Count; i++)
                    yield return _readBuff[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
