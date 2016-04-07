﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class DataSeriesProxy<T> : IDataSeriesProxy, Api.DataSeries<T>
    {
        public DataSeriesProxy()
        {
            DefaultValue = Metadata.EmptyValue.Get<T>();
        }

        public virtual IPluginDataBuffer<T> Buffer { get; set; }
        public virtual T DefaultValue { get; set; }
        public bool Readonly { get; set; }

        public int Count { get { return Buffer.VirtualPos; } }

        object IDataSeriesProxy.Buffer { get { return Buffer; } }

        public virtual T this[int index]
        {
            get
            {
                int readlIndex = GetRealIndex(index);

                if (IsOutOfBoundaries(readlIndex))
                    return DefaultValue;

                return Buffer[readlIndex];
            }

            set
            {
                int realIndex = GetRealIndex(index);

                if (Readonly || IsOutOfBoundaries(realIndex))
                    return;

                Buffer[realIndex] = value;
            }
        }

        private int GetRealIndex(int virtualIndex)
        {
            return Count - virtualIndex - 1;
        }

        private bool IsOutOfBoundaries(int index)
        {
            return index < 0 || index >= Buffer.Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (i < Buffer.Count)
                    yield return Buffer[i];
                else
                    yield return DefaultValue;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
