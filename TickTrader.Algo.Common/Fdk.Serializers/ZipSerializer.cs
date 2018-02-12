using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using TickTrader.BusinessObjects.QuoteHistory;
using TickTrader.BusinessObjects.QuoteHistory.Store;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;

namespace TickTrader.Server.QuoteHistory.Serialization
{
    public class ItemsZipSerializer<T, TList> : IItemsSerializer<T, TList> where TList : IList<T>, new()
    {
        private const string SerializationExceptionMessage = "Serialization failed.";
        private const string DeserializationExceptionMessage = "Deserialization failed.";

        private static readonly Encoding TextEncoding = Encoding.ASCII;

        private IFormatter<T> stringFormatter;
        private readonly string textFileName;
        private readonly string zipFileName;

        public IFormatter<T> Formatter
        {
            get { return stringFormatter; }
            set { stringFormatter = value; }
        }

        public ItemsZipSerializer(IFormatter<T> stringFormatter, string fileNameWithoutExtension)
        {
            this.stringFormatter = stringFormatter;
            this.textFileName = fileNameWithoutExtension + ".txt";
            this.zipFileName = fileNameWithoutExtension + ".zip";
        }
        
        public string FileName
        {
            get { return this.zipFileName; }
        }

        public byte[] Serialize(IEnumerable<T> items)
        {
            Crc32Hash crc;
            return this.Serialize(items, out crc);
        }

        public byte[] Zip(byte[] dataToZip)
        {
            using (var target = new MemoryStream())
            {
                try
                {
                    using (var zipOutputStream = new ZipOutputStream(target))
                    {
                        zipOutputStream.UseZip64 = UseZip64.Off;
                        zipOutputStream.SetLevel(1);
                        var zipEntry = new ZipEntry(this.textFileName);
                        zipOutputStream.PutNextEntry(zipEntry);
                        using (var streamWriter = new StreamWriter(zipOutputStream, TextEncoding))
                        {
                            streamWriter.Write(dataToZip);
                            streamWriter.Flush();
                            zipOutputStream.CloseEntry();
                        }
                    }
                    return target.ToArray();
                }
                catch (SystemException e)
                {
                    throw new SerializationException(SerializationExceptionMessage, e);
                }
            }
            
        }

        public byte[] Serialize(IEnumerable<T> items, out Crc32Hash uncompressedCrc)
        {
            using (var target = new MemoryStream())
            {
                try
                {
                    using (var zipOutputStream = new ZipOutputStream(target))
                    {
                        zipOutputStream.UseZip64 = UseZip64.Off;
                        zipOutputStream.SetLevel(1);
                        var zipEntry = new ZipEntry(this.textFileName);
                        zipOutputStream.PutNextEntry(zipEntry);
                        using (var streamWriter = new StreamWriter(zipOutputStream, TextEncoding))
                        {
                            //streamWriter.Write(this.stringFormatter.Serialize(items));
                            stringFormatter.Serialize(streamWriter, items);
                            streamWriter.Flush();
                            zipOutputStream.CloseEntry();
                        }
                        uncompressedCrc = Crc32Hash.Parse(zipEntry.Crc);
                    }
                    return target.ToArray();
                }
                catch (SystemException e)
                {
                    throw new SerializationException(SerializationExceptionMessage, e);
                }
            }
        }

        public TList Deserialize(byte[] bytes)
        {
            Crc32Hash crc;
            return this.Deserialize(bytes, out crc);
        }

        public TList Deserialize(byte[] bytes, out Crc32Hash uncompressedCrc)
        {
            //File.WriteAllBytes(string.Format(@"d:\TEMP\{0}_temp.dat", DateTime.Now.Ticks), bytes);

            using (var source = new MemoryStream(bytes))
            {
                string line = "";

                try
                {
                    using (var zipFile = new ZipFile(source))
                    {
                        var zipEntry = zipFile.GetEntry(this.textFileName);
                        if (zipEntry == null)
                        {
                            throw new SerializationException(string.Format("{0} Cannot find {1} inside zip file.", DeserializationExceptionMessage, this.textFileName));
                        }

                        var elements = new TList();

                        using (var zipInputStream = zipFile.GetInputStream(zipEntry))
                        using (var streamReader = new StreamReader(zipInputStream, TextEncoding))
                        {
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                var element = this.stringFormatter.Deserialize(line);
                                elements.Add(element);
                            }
                        }
                        uncompressedCrc = Crc32Hash.Parse(zipEntry.Crc);
                        return elements;
                    }
                }
                catch (SharpZipBaseException e)
                {
                    throw new SerializationException(DeserializationExceptionMessage, e);
                }
                catch (FormatException e)
                {
                    throw new InvalidDataException(DeserializationExceptionMessage + "Can't deserialize line: [" + line + "]", e);
                }
            }
        }

        public ChunkMetaInfo.SerializationMethod SerializerType
        {
            get { return ChunkMetaInfo.SerializationMethod.Zip; }
        }
    }
}
