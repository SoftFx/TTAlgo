using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class BarVector : BarVectorBase
    {
        private readonly List<BarEntity> _list = new List<BarEntity>();

        public BarVector(TimeFrames timeFrame) : base(timeFrame)
        {
        }

        public override BarEntity Last
        {
            get { return _list[_list.Count - 1]; }
            set { _list[_list.Count - 1] = value; }
        }

        public BarEntity First
        {
            get { return _list[0]; }
            set { _list[0] = value; }
        }

        public int Count => _list.Count;
        public override bool HasElements => _list.Count > 0;

        public BarEntity[] ToArray()
        {
            return _list.ToArray();
        }

        public BarEntity[] RemoveFromStart(int count)
        {
            if (Count < count)
                throw new InvalidOperationException("Not enough bars in vector!");

            var range = new BarEntity[count];
            for (int i = 0; i < count; i++)
                range[i] = _list[i];
            _list.RemoveRange(0, count);
            return range;
        }

        public void Clear()
        {
            _list.Clear();
        }

        protected override void AddToVector(BarEntity entity)
        {
            _list.Add(entity);
        }

        private void InternalAdd(BarEntity bar)
        {
            _list.Add(bar);
        }
    }
}
