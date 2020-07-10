using Machinarium.Var;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.UnitTest
{
    [TestClass]
    public class SelectOperatorTest
    {
        [TestMethod]
        public void SelectOperator_ChangeRef()
        {
            var entity1 = new MockEntity(10);
            var entity2 = new MockEntity(11);
            var prop = new Property<MockEntity>();

            prop.Value = entity1;

            var intRef = prop.Ref(p => p.Prop);

            Assert.AreEqual(10, intRef.Value.Value);

            prop.Value = entity2;

            Assert.AreEqual(11, intRef.Value.Value);
        }

        [TestMethod]
        public void SelectOperator_ChangeEntitProp()
        {
            var entity1 = new MockEntity(10);
            var prop = new Property<MockEntity>();

            prop.Value = entity1;

            var intRef = prop.Ref(p => p.Prop);

            Assert.AreEqual(10, intRef.Value.Value);

            entity1.Change(11);

            Assert.AreEqual(11, intRef.Value.Value);
        }

        [TestMethod]
        public void SelectOperator_NullRef()
        {
            var entity1 = new MockEntity(10);
            var prop = new Property<MockEntity>();

            var intRef = prop.Ref(p => p.Prop);

            Assert.AreEqual(null, intRef.Value);

            prop.Value = entity1;

            Assert.AreNotEqual(null, intRef.Value);
            Assert.AreEqual(10, intRef.Value.Value);

            prop.Value = null;

            Assert.AreEqual(null, intRef.Value);
        }

        public class MockEntity : EntityBase
        {
            private IntProperty _prop;

            public MockEntity(int propValue)
            {
                _prop = AddIntProperty(propValue);
            }

            public IntVar Prop => _prop.Var;

            public void Change(int newVal)
            {
                _prop.Value = newVal;
            }
        }
    }
}
