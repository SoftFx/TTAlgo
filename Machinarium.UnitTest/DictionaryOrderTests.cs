using Machinarium.Qnil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.UnitTest
{
    [TestClass]
    public class DictionaryOrderTests
    {
        [TestMethod]
        public void DictionaryOrder_AddRemove()
        {
            var set = new VarDictionary<string, string>();
            var list = set.OrderBy((k, v) => k);

            set.Add("1", "one");
            set.Add("2", "two");
            set.Remove("1");
            set.Add("3", "three");
            set.Remove("3");
            set.Add("4", "four");

            Assert.AreEqual(2, list.Snapshot.Count);
            Assert.AreEqual("two", list.Snapshot[0]);
            Assert.AreEqual("four", list.Snapshot[1]);
        }
    }
}
