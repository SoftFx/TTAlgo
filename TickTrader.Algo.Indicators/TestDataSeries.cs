using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    class TestDataSeries : DataSeries
    {

        public List<double> ser;

        public TestDataSeries()
        {
            ser = new List<double>();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)ser.GetEnumerator();
        }
        IEnumerator<double> IEnumerable<double>.GetEnumerator()
        {
            return (IEnumerator<double>)ser.GetEnumerator();
        }

        int DataSeries<double>.Count
        {
            get
            {
                return ser.Count;
            }
        }

        double DataSeries<double>.this[int index]
        {
            get { return (double)ser[index]; }

            set { ser[index] = value; }
        }

    }
}
