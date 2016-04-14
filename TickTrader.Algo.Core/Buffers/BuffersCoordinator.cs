using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class BuffersCoordinator
    {
        public event Action BuffersExtended = delegate { };
        public event Action BuffersShifted = delegate { };
        public event Action BuffersCleared = delegate { };

        public int VirtualPos { get; private set; }
        public int MaxBufferSize { get; set; }

        public void MoveNext()
        {
            if (MaxBufferSize > 0 && VirtualPos >= MaxBufferSize)
                BuffersShifted();
            else
            {
                VirtualPos++;
                BuffersExtended();
            }
        }

        //public void Reset()
        //{
        //    BuffersCleared();
        //    VirtualPos = 0;
        //}
    }
}
