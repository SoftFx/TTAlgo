using Machinarium.Qnil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Machinarium.Tests
{
    [TestClass]
    public class ListItemDisposeTests
    {
        internal class DisposableItem : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose() => Disposed = true;
        }


        [TestMethod]
        public void CheckDisposeOnAddRemove()
        {
            var items = new DisposableItem[7];
            for (var i = 0; i < items.Length; i++)
                items[i] = new DisposableItem();

            var src = new VarList<int>();
            for (var i = 0; i < 5; i++)
                src.Add(i);

            var list = src.Select(index => items[index]).DisposeItems();

            Assert.AreEqual(list.Snapshot.Count, 5);
            Assert.AreEqual(list.Snapshot.Any(it => it.Disposed), false);

            src.RemoveAt(1);
            Assert.AreEqual(items[1].Disposed, true);
            src.RemoveAt(1);
            Assert.AreEqual(items[2].Disposed, true);

            src.Add(5);
            src.Add(6);
            
            Assert.AreEqual(list.Snapshot.Count, 5);
            Assert.AreEqual(list.Snapshot.Any(it => it.Disposed), false);
        }

        [TestMethod]
        public void CheckDisposeOnReplace()
        {
            var items = new DisposableItem[7];
            for (var i = 0; i < items.Length; i++)
                items[i] = new DisposableItem();

            var src = new VarList<int>();
            for (var i = 0; i < 5; i++)
                src.Add(i);

            var list = src.Select(index => items[index]).DisposeItems();

            Assert.AreEqual(list.Snapshot.Count, 5);
            Assert.AreEqual(list.Snapshot.Any(it => it.Disposed), false);

            for (var i = 0; i < src.Count; i++)
            {
                // this will trigger event when oldItem and newItem are same object
                // in such cases we should skip dispose
                src[i] = i;
            }

            Assert.AreEqual(list.Snapshot.Count, 5);
            Assert.AreEqual(list.Snapshot.Any(it => it.Disposed), false);

            src[1] = 5;
            Assert.AreEqual(items[1].Disposed, true);
            src[3] = 6;
            Assert.AreEqual(items[3].Disposed, true);

            Assert.AreEqual(list.Snapshot.Count, 5);
            Assert.AreEqual(list.Snapshot.Any(it => it.Disposed), false);
        }

        [TestMethod]
        public void CheckDisposeOnClear()
        {
            var items = new DisposableItem[5];
            for (var i = 0; i < items.Length; i++)
                items[i] = new DisposableItem();

            var src = new VarList<int>();
            for (var i = 0; i < 5; i++)
                src.Add(i);

            var list = src.Select(index => items[index]).DisposeItems();

            Assert.AreEqual(list.Snapshot.Count, 5);
            Assert.AreEqual(list.Snapshot.Any(it => it.Disposed), false);

            src.Clear();
            for (var i = 0; i < items.Length; i++)
            {
                if (!items[i].Disposed)
                    throw new AssertFailedException($"Object at index {i} not disposed");
            }
        }

        [TestMethod]
        public void CheckDisposeOnWrapperDispose()
        {
            var items = new DisposableItem[5];
            for (var i = 0; i < items.Length; i++)
                items[i] = new DisposableItem();

            var src = new VarList<int>();
            for (var i = 0; i < 5; i++)
                src.Add(i);

            var list = src.Select(index => items[index]).DisposeItems();

            Assert.AreEqual(list.Snapshot.Count, 5);
            Assert.AreEqual(list.Snapshot.Any(it => it.Disposed), false);

            list.Dispose();
            for (var i = 0; i < items.Length; i++)
            {
                if (!items[i].Disposed)
                    throw new AssertFailedException($"Object at index {i} not disposed");
            }
        }
    }
}
