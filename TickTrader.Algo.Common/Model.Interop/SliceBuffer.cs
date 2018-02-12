using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    internal abstract class SliceBuffer<T> : AsyncBuffer<Slice<T>>
    {
        private DateTime _from;
        private DateTime _to;
        private T[] _cachedPage;

        public SliceBuffer(DateTime from, DateTime to)
        {
            _from = from;
            _to = to;
        }

        protected abstract Slice<T> CreateSlice(DateTime sliceForm, DateTime sliceTo, T[] sliceContent);
        protected abstract DateTime GetTime(T entity);

        public bool Write(T[] page)
        {
            return WriteAsync(page).Result;
        }

        public Task<bool> WriteAsync(T[] page)
        {
            if (_cachedPage == null)
            {
                // just cache first page
                _cachedPage = page;
                return Task.FromResult(true);
            }
            else
            {
                var sliceStart = _from;
                var sliceEnd = GetTime(page.First());
                var slice = CreateSlice(sliceStart, sliceEnd, _cachedPage);

                _from = sliceEnd;
                _cachedPage = page;

                return WriteAsync(slice);
            }
        }

        public bool CompleteWrite()
        {
            return CompleteWriteAsync().Result;
        }

        public Task<bool> CompleteWriteAsync()
        {
            if (_cachedPage != null)
            {
                var slice = CreateSlice(_from, _to, _cachedPage);
                _cachedPage = null;
                return WriteAsync(slice);
            }
            else
                return Task.FromResult(true);
        }
    }

    internal class BarSliceBuffer : SliceBuffer<BarEntity>
    {
        public BarSliceBuffer(DateTime from, DateTime to) : base(from, to)
        {
        }

        protected override Slice<BarEntity> CreateSlice(DateTime sliceForm, DateTime sliceTo, BarEntity[] sliceContent)
        {
            return new Slice<BarEntity>(sliceForm, sliceTo, sliceContent);
        }

        protected override DateTime GetTime(BarEntity entity)
        {
            return entity.OpenTime;
        }
    }

    internal class QuoteSliceBuffer : SliceBuffer<QuoteEntity>
    {
        public QuoteSliceBuffer(DateTime from, DateTime to) : base(from, to)
        {
        }

        protected override Slice<QuoteEntity> CreateSlice(DateTime sliceForm, DateTime sliceTo, QuoteEntity[] sliceContent)
        {
            return new Slice<QuoteEntity>(sliceForm, sliceTo, sliceContent);
        }

        protected override DateTime GetTime(QuoteEntity entity)
        {
            return entity.CreatingTime;
        }
    }
}
