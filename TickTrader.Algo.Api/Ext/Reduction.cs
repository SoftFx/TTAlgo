using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api.Ext
{
    public interface BarToDoubleReduction
    {
        double Reduce(Bar bar);
    }

    public interface FullBarToDoubleReduction
    {
        double Reduce(Bar bidBar, Bar askBar);
    }

    public interface QuoteToDoubleReduction
    {
        double Reduce(Quote quote);
    }

    public interface FullBarToBarReduction
    {
        void Reduce(Bar bidBar, Bar askBar, IBarWriter result);
    }

    public interface IBarWriter
    {
        double High { set; }
        double Low { set; } 
        double Open { set; }
        double Close { set; }
        double Volume { set; }  
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ReductionAttribute : Attribute
    {
        public ReductionAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }
}
