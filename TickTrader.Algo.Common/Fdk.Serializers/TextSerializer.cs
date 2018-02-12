using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using TickTrader.BusinessObjects.QuoteHistory;
using TickTrader.BusinessObjects.QuoteHistory.Store;

namespace TickTrader.Server.QuoteHistory.Serialization
{
    public class ItemsTextSerializer<T, TList> : IItemsSerializer<T, TList> where TList : IList<T>, new()
    {
        private const string SerializationExceptionMessage = "Serialization failed.";
        private const string DeserializationExceptionMessage = "Deserialization failed.";

        private static readonly Encoding TextEncoding = Encoding.ASCII;

        private IFormatter<T> _stringFormatter;
        private readonly string _textFileName;

        public IFormatter<T> Formatter
        {
            get { return _stringFormatter; }
            set { _stringFormatter = value; }
        }

        public ItemsTextSerializer(IFormatter<T> stringFormatter, string fileNameWithoutExtension)
        {
            _stringFormatter = stringFormatter;
            _textFileName = fileNameWithoutExtension + ".txt";
        }

        public string FileName
        {
            get { return _textFileName; }
        }

        public byte[] Serialize(IEnumerable<T> items)
        {
            Crc32Hash crc;
            return Serialize(items, out crc);
        }

        public byte[] Serialize(IEnumerable<T> items, out Crc32Hash crc)
        {
            try
            {
                var data = TextEncoding.GetBytes(_stringFormatter.Serialize(items));

                crc = Crc32HashExtension.Compute(data);
                return data;
            }
            catch (SystemException e)
            {
                throw new SerializationException(SerializationExceptionMessage, e);
            }
        }

        public TList Deserialize(byte[] bytes)
        {
            Crc32Hash crc;
            return Deserialize(bytes, out crc);
        }

        public TList Deserialize(byte[] bytes, out Crc32Hash crc)
        {
            using (var source = new MemoryStream(bytes))
            {
                try
                {
                    var elements = new TList();

                    using (var streamReader = new StreamReader(source, TextEncoding))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var element = _stringFormatter.Deserialize(line);
                            elements.Add(element);
                        }
                    }
                    crc = Crc32HashExtension.Compute(bytes);
                    return elements;
                }
                catch (FormatException e)
                {
                    throw new InvalidDataException(DeserializationExceptionMessage, e);
                }
            }
        }

        public ChunkMetaInfo.SerializationMethod SerializerType
        {
            get { return ChunkMetaInfo.SerializationMethod.Text; }
        }
    }
}
