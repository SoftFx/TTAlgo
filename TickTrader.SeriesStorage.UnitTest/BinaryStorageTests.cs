using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.SeriesStorage.LevelDb;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using TickTrader.SeriesStorage.Lmdb;
using System.IO;

namespace TickTrader.SeriesStorage.UnitTest
{
    [TestClass]
    public abstract class BinaryStorageTests
    {
        private readonly static IKeySerializer<DateTime> keySerializer = new DatTimeKeySerializer();

        protected string WorkingFolder;

        [TestInitialize]
        public void Init()
        {
            WorkingFolder = Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(WorkingFolder);
        }

        [TestCleanup]
        public void Deinit()
        {
            Directory.Delete(WorkingFolder, true);
        }

        // Setup 1:

        //    collection1
        //    2017, 08, 01 - 1
        //    2017, 08, 05 - 2
        //    2017, 08, 15 - 3

        //    collection2
        //    2017, 08, 01 - 4
        //    2017, 08, 05 - 5
        //    2017, 08, 15 - 6

        //    collection3
        //    2017, 08, 01 - 7
        //    2017, 08, 05 - 8
        //    2017, 08, 15 - 9

        protected abstract ISeriesDatabase OpenDatabase();
        //protected abstract void DestroyDatabase(string dbPath);

        private void WriteSetup1()
        {
            ClearWorkingFolder();

            using (var db = OpenDatabase())
            {
                using (var collection1 = db.GetBinaryCollection("collection1", keySerializer))
                {
                    collection1.Write(new DateTime(2017, 08, 01), ByteSegment(1));
                    collection1.Write(new DateTime(2017, 08, 05), ByteSegment(2));
                    collection1.Write(new DateTime(2017, 08, 15), ByteSegment(3));
                }

                using (var collection2 = db.GetBinaryCollection("collection2", keySerializer))
                {
                    collection2.Write(new DateTime(2017, 08, 15), ByteSegment(6));
                    collection2.Write(new DateTime(2017, 08, 05), ByteSegment(5));
                    collection2.Write(new DateTime(2017, 08, 01), ByteSegment(4));
                }

                using (var collection3 = db.GetBinaryCollection("collection3", keySerializer))
                {
                    collection3.Write(new DateTime(2017, 08, 05), ByteSegment(8));
                    collection3.Write(new DateTime(2017, 08, 15), ByteSegment(9));
                    collection3.Write(new DateTime(2017, 08, 01), ByteSegment(7));
                }
            }
        }

        private void WriteSetup2()
        {
        }

        private void ClearWorkingFolder()
        {
            EmptyFolder(new DirectoryInfo(WorkingFolder));
        }

        [TestMethod]
        public void BinaryStorage_WriteRead()
        {
            WriteSetup1();

            using (var db = OpenDatabase())
            {
                var list1 = ReadCollection(db, "collection1");

                Assert.AreEqual(3, list1.Count);
                Assert.AreEqual(new DateTime(2017, 08, 01), list1[0].Key);
                Assert.AreEqual(new DateTime(2017, 08, 05), list1[1].Key);
                Assert.AreEqual(new DateTime(2017, 08, 15), list1[2].Key);

                AssertCollectionsAreEqual(ByteSegment(1), list1[0].Value);
                AssertCollectionsAreEqual(ByteSegment(2), list1[1].Value);
                AssertCollectionsAreEqual(ByteSegment(3), list1[2].Value);

                var list2 = ReadCollection(db, "collection2");

                Assert.AreEqual(3, list2.Count);
                Assert.AreEqual(new DateTime(2017, 08, 01), list2[0].Key);
                Assert.AreEqual(new DateTime(2017, 08, 05), list2[1].Key);
                Assert.AreEqual(new DateTime(2017, 08, 15), list2[2].Key);

                AssertCollectionsAreEqual(ByteSegment(4), list2[0].Value);
                AssertCollectionsAreEqual(ByteSegment(5), list2[1].Value);
                AssertCollectionsAreEqual(ByteSegment(6), list2[2].Value);

                var list3 = ReadCollection(db, "collection3");

                Assert.AreEqual(3, list3.Count);
                Assert.AreEqual(new DateTime(2017, 08, 01), list3[0].Key);
                Assert.AreEqual(new DateTime(2017, 08, 05), list3[1].Key);
                Assert.AreEqual(new DateTime(2017, 08, 15), list3[2].Key);

                AssertCollectionsAreEqual(ByteSegment(7), list3[0].Value);
                AssertCollectionsAreEqual(ByteSegment(8), list3[1].Value);
                AssertCollectionsAreEqual(ByteSegment(9), list3[2].Value);
            }
        }

        [TestMethod]
        public void BinaryStorage_Seek_Exact()
        {
            WriteSetup1();

            using (var db = OpenDatabase())
            {
                using (var collection = db.GetBinaryCollection("collection1", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 08, 05)).ToList();
                    Assert.AreEqual(2, forward.Count);
                    AssertCollectionsAreEqual(forward[0].Value, Bytes(2));
                    AssertCollectionsAreEqual(forward[1].Value, Bytes(3));

                    var backward = collection.Iterate(new DateTime(2017, 08, 05), true).ToList();
                    Assert.AreEqual(2, backward.Count);
                    AssertCollectionsAreEqual(backward[0].Value, Bytes(2));
                    AssertCollectionsAreEqual(backward[1].Value, Bytes(1));
                }

                using (var collection = db.GetBinaryCollection("collection2", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 08, 05)).ToList();
                    Assert.AreEqual(2, forward.Count);
                    AssertCollectionsAreEqual(forward[0].Value, Bytes(5));
                    AssertCollectionsAreEqual(forward[1].Value, Bytes(6));

                    var backward = collection.Iterate(new DateTime(2017, 08, 05), true).ToList();
                    Assert.AreEqual(2, backward.Count);
                    AssertCollectionsAreEqual(backward[0].Value, Bytes(5));
                    AssertCollectionsAreEqual(backward[1].Value, Bytes(4));
                }

                using (var collection = db.GetBinaryCollection("collection3", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 08, 05)).ToList();
                    Assert.AreEqual(2, actual.Count);
                    AssertCollectionsAreEqual(actual[0].Value, Bytes(8));
                    AssertCollectionsAreEqual(actual[1].Value, Bytes(9));

                    var backward = collection.Iterate(new DateTime(2017, 08, 05), true).ToList();
                    Assert.AreEqual(2, backward.Count);
                    AssertCollectionsAreEqual(backward[0].Value, Bytes(8));
                    AssertCollectionsAreEqual(backward[1].Value, Bytes(7));
                }
            }
        }

        [TestMethod]
        public void BinaryStorage_Seek_Above()
        {
            WriteSetup1();

            using (var db = OpenDatabase())
            {
                using (var collection = db.GetBinaryCollection("collection1", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 08, 16)).ToList();
                    Assert.AreEqual(1, forward.Count);
                    AssertCollectionsAreEqual(forward[0].Value, Bytes(3));

                    var backward = collection.Iterate(new DateTime(2017, 08, 16), true).ToList();
                    Assert.AreEqual(3, backward.Count);
                    AssertCollectionsAreEqual(backward[0].Value, Bytes(3));
                    AssertCollectionsAreEqual(backward[1].Value, Bytes(2));
                    AssertCollectionsAreEqual(backward[2].Value, Bytes(1));
                }

                using (var collection = db.GetBinaryCollection("collection2", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 08, 16)).ToList();
                    Assert.AreEqual(1, forward.Count);
                    AssertCollectionsAreEqual(forward[0].Value, Bytes(6));

                    var backward = collection.Iterate(new DateTime(2017, 08, 16), true).ToList();
                    Assert.AreEqual(3, backward.Count);
                    AssertCollectionsAreEqual(backward[0].Value, Bytes(6));
                    AssertCollectionsAreEqual(backward[1].Value, Bytes(5));
                    AssertCollectionsAreEqual(backward[2].Value, Bytes(4));
                }

                using (var collection = db.GetBinaryCollection("collection3", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 08, 16)).ToList();
                    Assert.AreEqual(1, forward.Count);
                    AssertCollectionsAreEqual(forward[0].Value, Bytes(9));

                    var backward = collection.Iterate(new DateTime(2017, 08, 16), true).ToList();
                    Assert.AreEqual(3, backward.Count);
                    AssertCollectionsAreEqual(backward[0].Value, Bytes(9));
                    AssertCollectionsAreEqual(backward[1].Value, Bytes(8));
                    AssertCollectionsAreEqual(backward[2].Value, Bytes(7));
                }
            }
        }

        [TestMethod]
        public void BinaryStorage_Seek_Below()
        {
            WriteSetup1();

            using (var db = OpenDatabase())
            {
                using (var collection = db.GetBinaryCollection("collection1", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(3, forward.Count);
                    AssertCollectionsAreEqual(forward[0].Value, Bytes(1));
                    AssertCollectionsAreEqual(forward[1].Value, Bytes(2));
                    AssertCollectionsAreEqual(forward[2].Value, Bytes(3));

                    var backward = collection.Iterate(new DateTime(2017, 07, 30), true).ToList();
                    Assert.AreEqual(0, backward.Count);
                }

                using (var collection = db.GetBinaryCollection("collection2", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(3, forward.Count);
                    AssertCollectionsAreEqual(forward[0].Value, Bytes(4));
                    AssertCollectionsAreEqual(forward[1].Value, Bytes(5));
                    AssertCollectionsAreEqual(forward[2].Value, Bytes(6));

                    var backward = collection.Iterate(new DateTime(2017, 07, 30), true).ToList();
                    Assert.AreEqual(0, backward.Count);
                }

                using (var collection = db.GetBinaryCollection("collection3", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(3, forward.Count);
                    AssertCollectionsAreEqual(forward[0].Value, Bytes(7));
                    AssertCollectionsAreEqual(forward[1].Value, Bytes(8));
                    AssertCollectionsAreEqual(forward[2].Value, Bytes(9));

                    var backward = collection.Iterate(new DateTime(2017, 07, 30), true).ToList();
                    Assert.AreEqual(0, backward.Count);
                }
            }
        }

        [TestMethod]
        public void BinaryStorage_Seek_Empty()
        {
            WriteSetup2();

            using (var db = OpenDatabase())
            {
                using (var collection = db.GetBinaryCollection("collection1", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(0, forward.Count);

                    var backward = collection.Iterate(new DateTime(2017, 07, 30), true).ToList();
                    Assert.AreEqual(0, backward.Count);
                }

                using (var collection = db.GetBinaryCollection("collection3", keySerializer))
                {
                    var forward = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(0, forward.Count);

                    var backward = collection.Iterate(new DateTime(2017, 07, 30), true).ToList();
                    Assert.AreEqual(0, backward.Count);
                }
            }
        }

        public static ArraySegment<byte> ByteSegment(params byte[] bytes)
        {
            return new ArraySegment<byte>(bytes);
        }

        public static byte[] Bytes(params byte[] bytes)
        {
            return bytes;
        }

        [System.Diagnostics.DebuggerHidden]
        private static void AssertCollectionsAreEqual<T>(ICollection<T> collection1, ICollection<T> collection2)
        {
            CollectionAssert.AreEqual(collection1.ToArray(), collection2.ToArray());
        }

        private List<KeyValuePair<DateTime, ArraySegment<byte>>> ReadCollection(ISeriesDatabase db, string collectionName)
        {
            using (var collection = db.GetBinaryCollection(collectionName, keySerializer))
                return collection.Iterate().ToList();
        }

        private void EmptyFolder(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
            {
                EmptyFolder(subfolder);
            }
        }
    }

    [TestClass]
    public class LevelDbStorageTests : BinaryStorageTests
    {
        protected override ISeriesDatabase OpenDatabase()
        {
            return SeriesDatabase.Create(new LevelDbStorage(WorkingFolder));
        }
    }

    [TestClass]
    public class LightningDbStorageTests : BinaryStorageTests
    {
        protected override ISeriesDatabase OpenDatabase()
        {
            var manager = new LmdbManager(WorkingFolder);
            return SeriesDatabase.Create(manager);
        }
    }
}
