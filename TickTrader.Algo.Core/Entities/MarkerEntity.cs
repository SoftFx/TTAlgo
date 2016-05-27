using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Entities
{
    [Serializable]
    public class MarkerEntity : Marker, IFixedEntry<Marker>
    {
        private static Action<Marker> emptyHandler = e => { };

        private double y = double.NaN;
        private Colors color = Colors.Auto;
        private string text;
        private MarkerIcons icon;
        //private MarkerAlignments aligment;
        private Dictionary<string, string> properties = new Dictionary<string, string>();
        [NonSerialized]
        private Action<Marker> changed = emptyHandler;

        public Colors Color
        {
            get { return color; }
            set
            {
                color = value;
                Changed(this);
            }
        }

        public IDictionary<string, string> DisplayProperties { get; private set; }

        public string DisplayText
        {
            get { return text; }
            set
            {
                text = Normalize(value);
                Changed(this);
            }
        }

        public MarkerIcons Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                Changed(this);
            }
        }

        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                Changed(this);
            }
        }

        public Action<Marker> Changed
        {
            get { return changed; }
            set
            {
                if (changed == null)
                    throw new InvalidOperationException("Changed cannot be null!");
                changed = value;
            }
        }

        //public MarkerAlignments Alignment
        //{
        //    get { return aligment; }
        //    set
        //    {
        //        aligment = value;
        //        Changed(this);
        //    }
        //}

        public void Clear()
        {
            Reset();
            Changed(this);
        }

        public void CopyFrom(Marker val)
        {
            if (val == null)
                Reset();
            else
            {
                y = val.Y;
                color = val.Color;
                text = Normalize(val.DisplayText);
                icon = val.Icon;
            }
            Changed(this);
        }

        private string Normalize(string input)
        {
            if (input == null)
                return string.Empty;
            return input;
        }

        private void Reset()
        {
            y = double.NaN;
            color = Colors.Auto;
            text = string.Empty;
            icon = MarkerIcons.Circle;
        }
    }
}
