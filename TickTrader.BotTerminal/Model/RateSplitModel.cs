using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class RateSplitModel : PropertyChangedBase
    {
        private double? rate;
        private int digits;

        public int Digits
        {
            get { return digits; }
            set
            {
                this.digits = value;
                DoSplit();
            }
        }

        public double? Rate
        {
            get { return rate; }
            set
            {
                this.rate = value;
                DoSplit();
            }
        }

        private void DoSplit()
        {
            StringBuilder builder = new StringBuilder();
        }

        public string FirstPart { get; private set; }
        public string MiddlePart { get; private set; }
        public string LastPart { get; private set; }

    }
}
