using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Tests.Drawables
{
    [TestClass]
    public class DrawableCollectionTests
    {
        [TestMethod]
        public void TestCRUD()
        {
            (var ctx, var collection) = DrawableTestContext.Create();

            const string name = "Test object";
            var obj = collection.Create(name, Api.DrawableObjectType.TextBlockControl);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(true, ReferenceEquals(obj, collection[0]));
            Assert.AreEqual(true, collection.TryGetObject(name, out var objByName));
            Assert.AreEqual(true, ReferenceEquals(obj, objByName));

            Assert.AreEqual(0, ctx.Updates.Count);
            obj.PushChanges();

            Assert.AreEqual(1, ctx.Updates.Count);
            var upd = ctx.Updates[0];
            Assert.AreEqual(CollectionUpdate.Types.Action.Added, upd.Action);
            Assert.AreEqual(name, upd.ObjInfo.Name);
            Assert.AreEqual(Drawable.Types.ObjectType.TextBlockControl, upd.ObjInfo.Type);

            ctx.ResetUpdates();
            var oldVisibility = obj.Visibility;
            var newVisibility = Api.DrawableObjectVisibility.NoTimeframes;
            obj.Visibility = newVisibility;

            Assert.AreEqual(0, ctx.Updates.Count);
            obj.PushChanges();

            Assert.AreEqual(1, ctx.Updates.Count);
            upd = ctx.Updates[0];
            Assert.AreEqual(CollectionUpdate.Types.Action.Updated, upd.Action);
            Assert.AreEqual(name, upd.ObjInfo.Name);
            Assert.AreNotEqual((uint)oldVisibility, (uint)upd.ObjInfo.Visibility);
            Assert.AreEqual((uint)newVisibility, (uint)upd.ObjInfo.Visibility);

            ctx.ResetUpdates();
            collection.Remove(name);

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(1, ctx.Updates.Count);
            upd = ctx.Updates[0];
            Assert.AreEqual(CollectionUpdate.Types.Action.Removed, upd.Action);
            Assert.AreEqual(name, upd.ObjName);
        }


        [TestMethod]
        public void TestBatchAdd()
        {
            (var ctx, var collection) = DrawableTestContext.Create();

            const int cnt = 5;
            var objNames = DrawableTestContext.GetObjectNames(cnt, "TestObj");
            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);

            Assert.AreEqual(5, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);
            ctx.FlushUpdates();
            Assert.AreEqual(5, ctx.Updates.Count);
            for (var i = 0; i < cnt; i++)
            {
                Assert.AreEqual(objNames[i], ctx.Updates[i].ObjInfo.Name);
                Assert.AreEqual(CollectionUpdate.Types.Action.Added, ctx.Updates[i].Action);
            }
        }

        [TestMethod]
        public void TestExactlyOnceUpdate()
        {
            const string name = "Test object";
            (var ctx, var collection) = DrawableTestContext.Create();

            var obj1 = collection.Create(name + 1, Api.DrawableObjectType.TextBlockControl);
            obj1.Shape.BorderThickness = 120;
            obj1.Text.FontSize = 120;

            Assert.AreEqual(0, ctx.Updates.Count);
            obj1.PushChanges();
            Assert.AreEqual(1, ctx.Updates.Count);
            ctx.ResetUpdates();

            obj1.Shape.BorderThickness = 60;
            obj1.Text.FontSize = 60;

            Assert.AreEqual(0, ctx.Updates.Count);
            obj1.PushChanges();
            Assert.AreEqual(1, ctx.Updates.Count);
            ctx.ResetUpdates();

            var obj2 = collection.Create(name + 2, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();

            obj1.Shape.BorderThickness = 40;
            obj1.Text.FontSize = 40;
            obj2.Shape.BorderThickness = 30;
            obj2.Text.FontSize = 30;

            Assert.AreEqual(0, ctx.Updates.Count);
            obj1.PushChanges(); obj2.PushChanges();
            Assert.AreEqual(2, ctx.Updates.Count);
            ctx.ResetUpdates();
        }

        [TestMethod]
        public void TestBatchUpdate()
        {
            (var ctx, var collection) = DrawableTestContext.Create();

            const int cnt = 5;
            var objNames = DrawableTestContext.GetObjectNames(cnt, "TestObj");
            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();

            for (var i = 0; i < cnt; i++)
                collection[i].Tooltip = "test modify";

            Assert.AreEqual(0, ctx.Updates.Count);
            ctx.FlushUpdates();
            Assert.AreEqual(5, ctx.Updates.Count);
            for (var i = 0; i < cnt; i++)
            {
                Assert.AreEqual(objNames[i], ctx.Updates[i].ObjInfo.Name);
                Assert.AreEqual(CollectionUpdate.Types.Action.Updated, ctx.Updates[i].Action);
            }

            ctx.ResetUpdates();
            for (var i = 0; i < cnt; i++)
                for (var j = 0; j < cnt; j++)
                    collection[i].Tooltip = "test modify" + j;

            Assert.AreEqual(0, ctx.Updates.Count);
            ctx.FlushUpdates();
            Assert.AreEqual(5, ctx.Updates.Count);
            for (var i = 0; i < cnt; i++)
            {
                Assert.AreEqual(objNames[i], ctx.Updates[i].ObjInfo.Name);
                Assert.AreEqual(CollectionUpdate.Types.Action.Updated, ctx.Updates[i].Action);
            }
        }

        [TestMethod]
        public void TestPushChangesMethod()
        {
            (var ctx, var collection) = DrawableTestContext.Create();

            const int cnt = 5;
            var objNames = DrawableTestContext.GetObjectNames(cnt, "TestObj");
            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();

            for (var i = 0; i < cnt; i++)
                collection[i].Tooltip = "test modify";

            var index = 2;
            collection[index].PushChanges();
            Assert.AreEqual(1, ctx.Updates.Count);
            Assert.AreEqual(objNames[index], ctx.Updates[0].ObjInfo.Name);
            ctx.ResetUpdates();

            index = 4;
            collection[index].PushChanges();
            Assert.AreEqual(1, ctx.Updates.Count);
            Assert.AreEqual(objNames[index], ctx.Updates[0].ObjInfo.Name);
            ctx.ResetUpdates();

            ctx.FlushUpdates();
            Assert.AreEqual(3, ctx.Updates.Count);
        }

        [TestMethod]
        public void TestBatchRemove()
        {
            (var ctx, var collection) = DrawableTestContext.Create();

            const int cnt = 5;
            var objNames = DrawableTestContext.GetObjectNames(cnt, "TestObj");

            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);
            for (var i = 0; i < cnt; i++)
                collection.RemoveAt(0);
            ctx.FlushUpdates();
            Assert.AreEqual(0, ctx.Updates.Count);
            Assert.AreEqual(0, collection.Count);

            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();
            for (var i = 0; i < cnt; i++)
                collection.RemoveAt(0);
            Assert.AreEqual(5, ctx.Updates.Count);
            Assert.AreEqual(0, collection.Count);
            for (var i = 0; i < cnt; i++)
            {
                Assert.AreEqual(objNames[i], ctx.Updates[i].ObjName);
                Assert.AreEqual(CollectionUpdate.Types.Action.Removed, ctx.Updates[i].Action);
            }

            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();
            for (var i = 0; i < cnt; i++)
                collection.RemoveAt(collection.Count - 1);
            Assert.AreEqual(5, ctx.Updates.Count);
            Assert.AreEqual(0, collection.Count);
            for (var i = 0; i < cnt; i++)
            {
                Assert.AreEqual(objNames[cnt - i - 1], ctx.Updates[i].ObjName);
                Assert.AreEqual(CollectionUpdate.Types.Action.Removed, ctx.Updates[i].Action);
            }

            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();
            for (var i = 0; i < cnt; i++)
                collection.Remove(objNames[cnt - i - 1]);
            Assert.AreEqual(5, ctx.Updates.Count);
            Assert.AreEqual(0, collection.Count);
            for (var i = 0; i < cnt; i++)
            {
                Assert.AreEqual(objNames[cnt - i -1], ctx.Updates[i].ObjName);
                Assert.AreEqual(CollectionUpdate.Types.Action.Removed, ctx.Updates[i].Action);
            }
        }

        [TestMethod]
        public void TestClear()
        {
            (var ctx, var collection) = DrawableTestContext.Create();

            const int cnt = 5;
            var objNames = DrawableTestContext.GetObjectNames(cnt, "TestObj");

            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();
            Assert.AreEqual(5, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);
            collection.Clear();
            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(1, ctx.Updates.Count);
            Assert.AreEqual(CollectionUpdate.Types.Action.Cleared, ctx.Updates[0].Action);

            ctx.ResetUpdates();
            CreateNumberOfObjects(collection, cnt, objNames, Api.DrawableObjectType.TextBlockControl);
            Assert.AreEqual(5, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);
            collection.Clear();
            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(1, ctx.Updates.Count);
            Assert.AreEqual(CollectionUpdate.Types.Action.Cleared, ctx.Updates[0].Action);
        }

        [TestMethod]
        public void TestUpdateAfterDelete()
        {
            (var ctx, var collection) = DrawableTestContext.Create();
            const string name = "Test object";
            Api.IDrawableObject obj = null;

            obj = collection.Create(name, Api.DrawableObjectType.TextBlockControl);
            collection.Clear();
            ctx.ResetUpdates();
            obj.PushChanges();

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);

            obj = collection.Create(name, Api.DrawableObjectType.TextBlockControl);
            collection.Remove(name);
            obj.PushChanges();

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);

            obj = collection.Create(name, Api.DrawableObjectType.TextBlockControl);
            collection.RemoveAt(0);
            obj.PushChanges();

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);

            obj = collection.Create(name, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();
            obj.Tooltip = "test";
            collection.Clear();
            ctx.ResetUpdates();
            obj.PushChanges();

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);

            obj = collection.Create(name, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();
            obj.Tooltip = "test";
            collection.Remove(name);
            ctx.ResetUpdates();
            obj.PushChanges();

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);

            obj = collection.Create(name, Api.DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();
            obj.Tooltip = "test";
            collection.RemoveAt(0);
            ctx.ResetUpdates();
            obj.PushChanges();

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(0, ctx.Updates.Count);
        }


        private static void CreateNumberOfObjects(Api.IDrawableCollection collection, int cnt, string[] objNames, Api.DrawableObjectType type)
        {
            for (var i = 0; i < cnt; i++)
                _ = collection.Create(objNames[i], type);
        }
    }
}
