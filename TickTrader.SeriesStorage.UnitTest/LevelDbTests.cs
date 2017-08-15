using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TickTrader.SeriesStorage.LevelDb;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;

namespace TickTrader.SeriesStorage.UnitTest
{
    [TestClass]
    public class LevelDbTests
    {
        private readonly static IKeySerializer<DateTime> keySerializer = new DatTimeKeySerializer();
        private readonly static string tempdbPath = "test.db";

        [TestMethod]
        public void LevelDbStorage_SimpleWrite()
        {
            LevelDB.DB.Destroy(tempdbPath);

            using (var storage = new LevelDbStorage(tempdbPath))
            {
                using (var collection1 = storage.GetCollection("collection1", keySerializer))
                {
                    collection1.Write(new DateTime(2017, 08, 01), Bytes(1));
                    collection1.Write(new DateTime(2017, 08, 05), Bytes(2));
                    collection1.Write(new DateTime(2017, 08, 15), Bytes(3));
                }

                using (var collection2 = storage.GetCollection("collection2", keySerializer))
                {
                    collection2.Write(new DateTime(2017, 08, 15), Bytes(6));
                    collection2.Write(new DateTime(2017, 08, 05), Bytes(5));
                    collection2.Write(new DateTime(2017, 08, 01), Bytes(4));
                }

                using (var collection3 = storage.GetCollection("collection3", keySerializer))
                {
                    collection3.Write(new DateTime(2017, 08, 05), Bytes(8));
                    collection3.Write(new DateTime(2017, 08, 15), Bytes(9));
                    collection3.Write(new DateTime(2017, 08, 01), Bytes(7));
                }
            }

            using (var rawDb = new LevelDB.DB(tempdbPath))
            {
                var expected = Setup1();
                var actual = ReadAll(rawDb);
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void LevelDbStorage_Seek_Exact()
        {
            LevelDB.DB.Destroy(tempdbPath);

            using (var db = new LevelDB.DB(tempdbPath, new LevelDB.Options { CreateIfMissing = true }))
                db.Write(Setup1().ToLevelDbBatch());

            using (var storage = new LevelDbStorage(tempdbPath))
            {
                using (var collection = storage.GetCollection("collection1", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 08, 05)).ToList();
                    Assert.AreEqual(2, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(2));
                    CollectionAssert.AreEqual(actual[1].Value, Bytes(3));
                }

                using (var collection = storage.GetCollection("collection2", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 08, 05)).ToList();
                    Assert.AreEqual(2, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(5));
                    CollectionAssert.AreEqual(actual[1].Value, Bytes(6));
                }

                using (var collection = storage.GetCollection("collection3", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 08, 05)).ToList();
                    Assert.AreEqual(2, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(8));
                    CollectionAssert.AreEqual(actual[1].Value, Bytes(9));
                }
            }
        }

        [TestMethod]
        public void LevelDbStorage_Seek_Above()
        {
            LevelDB.DB.Destroy(tempdbPath);

            using (var db = new LevelDB.DB(tempdbPath, new LevelDB.Options { CreateIfMissing = true }))
                db.Write(Setup1().ToLevelDbBatch());

            using (var storage = new LevelDbStorage(tempdbPath))
            {
                using (var collection = storage.GetCollection("collection1", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 08, 16)).ToList();
                    Assert.AreEqual(1, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(3));
                }

                using (var collection = storage.GetCollection("collection2", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 08, 16)).ToList();
                    Assert.AreEqual(1, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(6));
                }

                using (var collection = storage.GetCollection("collection3", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 08, 16)).ToList();
                    Assert.AreEqual(1, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(9));
                }
            }
        }

        [TestMethod]
        public void LevelDbStorage_Seek_Below()
        {
            LevelDB.DB.Destroy(tempdbPath);

            using (var db = new LevelDB.DB(tempdbPath, new LevelDB.Options { CreateIfMissing = true }))
                db.Write(Setup1().ToLevelDbBatch());

            using (var storage = new LevelDbStorage(tempdbPath))
            {
                using (var collection = storage.GetCollection("collection1", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(3, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(1));
                    CollectionAssert.AreEqual(actual[1].Value, Bytes(2));
                    CollectionAssert.AreEqual(actual[2].Value, Bytes(3));
                }

                using (var collection = storage.GetCollection("collection2", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(3, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(4));
                    CollectionAssert.AreEqual(actual[1].Value, Bytes(5));
                    CollectionAssert.AreEqual(actual[2].Value, Bytes(6));
                }

                using (var collection = storage.GetCollection("collection3", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(3, actual.Count);
                    CollectionAssert.AreEqual(actual[0].Value, Bytes(7));
                    CollectionAssert.AreEqual(actual[1].Value, Bytes(8));
                    CollectionAssert.AreEqual(actual[2].Value, Bytes(9));
                }
            }
        }

        [TestMethod]
        public void LevelDbStorage_Seek_Empty()
        {
            LevelDB.DB.Destroy(tempdbPath);

            using (var db = new LevelDB.DB(tempdbPath, new LevelDB.Options { CreateIfMissing = true }))
                db.Write(Setup2().ToLevelDbBatch());

            using (var storage = new LevelDbStorage(tempdbPath))
            {
                using (var collection = storage.GetCollection("collection1", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(0, actual.Count);
                }

                using (var collection = storage.GetCollection("collection3", keySerializer))
                {
                    var actual = collection.Iterate(new DateTime(2017, 07, 30)).ToList();
                    Assert.AreEqual(0, actual.Count);
                }
            }
        }

        private static KeyValueSnapshot Setup1()
        {
            var snapshot = new KeyValueSnapshot();
            snapshot.Add(Bytes(0, 0, 0, 0, 1), Encoding.UTF8.GetBytes("collection1"));
            snapshot.Add(Bytes(0, 0, 0, 0, 2), Encoding.UTF8.GetBytes("collection2"));
            snapshot.Add(Bytes(0, 0, 0, 0, 3), Encoding.UTF8.GetBytes("collection3"));
            snapshot.Add(Bytes(0, 1, 8, 212, 216, 112, 64, 186, 192, 0), Bytes(1));
            snapshot.Add(Bytes(0, 1, 8, 212, 219, 148, 234, 97, 192, 0), Bytes(2));
            snapshot.Add(Bytes(0, 1, 8, 212, 227, 112, 146, 131, 64, 0), Bytes(3));
            snapshot.Add(Bytes(0, 2, 8, 212, 216, 112, 64, 186, 192, 0), Bytes(4));
            snapshot.Add(Bytes(0, 2, 8, 212, 219, 148, 234, 97, 192, 0), Bytes(5));
            snapshot.Add(Bytes(0, 2, 8, 212, 227, 112, 146, 131, 64, 0), Bytes(6));
            snapshot.Add(Bytes(0, 3, 8, 212, 216, 112, 64, 186, 192, 0), Bytes(7));
            snapshot.Add(Bytes(0, 3, 8, 212, 219, 148, 234, 97, 192, 0), Bytes(8));
            snapshot.Add(Bytes(0, 3, 8, 212, 227, 112, 146, 131, 64, 0), Bytes(9));
            return snapshot;
        }

        private static KeyValueSnapshot Setup2()
        {
            var snapshot = new KeyValueSnapshot();
            snapshot.Add(Bytes(0, 0, 0, 0, 1), Encoding.UTF8.GetBytes("collection1"));
            snapshot.Add(Bytes(0, 0, 0, 0, 2), Encoding.UTF8.GetBytes("collection2"));
            snapshot.Add(Bytes(0, 0, 0, 0, 3), Encoding.UTF8.GetBytes("collection3"));
            snapshot.Add(Bytes(0, 2, 8, 212, 219, 148, 234, 97, 192, 0), Bytes(5));
            return snapshot;
        }

        public static KeyValueSnapshot ReadAll(LevelDB.DB db)
        {
            var result = new KeyValueSnapshot();
            var records = (IEnumerable<KeyValuePair<byte[], byte[]>>)db;

            foreach (var r in records)
                result.Add(r.Key, r.Value);

            return result;
        }

        public static byte[] Bytes(params byte[] bytes)
        {
            return bytes;
        }

        public class DatTimeKeySerializer : IKeySerializer<DateTime>
        {
            public int KeySize => 8;

            public DateTime Deserialize(IKeyReader reader)
            {
                var ticks = reader.ReadBeLong();
                return new DateTime(ticks);
            }

            public void Serialize(DateTime key, IKeyBuilder builder)
            {
                builder.WriteBe(key.Ticks);
            }
        }

        public class KeyValueSnapshot : List<BinKeyValue>
        {
            public void Add(byte[] key, byte[] value)
            {
                Add(new BinKeyValue(key, value));
            }

            public LevelDB.WriteBatch ToLevelDbBatch()
            {
                var batch = new LevelDB.WriteBatch();
                foreach (var item in this)
                    batch.Put(item.Key, item.Value);
                return batch;
            }
        }

        public class BinKeyValue
        {
            public BinKeyValue(byte[] key, byte[] value)
            {
                Key = key;
                Value = value;
            }

            public byte[] Key { get; }
            public byte[] Value { get; }

            public override bool Equals(object obj)
            {
                var other = obj as BinKeyValue;
                return other != null
                    && Enumerable.SequenceEqual(other.Key, Key)
                    && Enumerable.SequenceEqual(other.Value, Value);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
