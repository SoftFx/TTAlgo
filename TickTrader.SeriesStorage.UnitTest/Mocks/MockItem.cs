using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage.UnitTest.Mocks
{
    internal class MockItem
    {
        public MockItem(int id, string val)
        {
            Id = id;
            Value = val;
        }

        public int Id { get; }
        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}
