﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.Algo.Package.V1
{
    public class Package
    {
        public const string MetadataFileName = "package.metadata.xml";
        public const string DefaultExtension = ".ttalgo";

        private readonly Dictionary<string, byte[]> _files;

        public PackageMetadata Metadata { get; private set; }


        public Package()
        {
            Metadata = new PackageMetadata();
            _files = new Dictionary<string, byte[]>();
        }

        protected Package(PackageMetadata metadata, Dictionary<string, byte[]> files)
        {
            Metadata = metadata;
            _files = files;
        }


        public bool AddFile(string path, byte[] fileBytes)
        {
            if (path.ToLower() == MetadataFileName) // in case of random metadata file in output
                return false;

            AddFileEntry(path, fileBytes);
            return true;
        }

        public byte[] GetFile(string path)
        {
            var normalizedPath = path.ToLowerInvariant();
            byte[] bytes;
            _files.TryGetValue(normalizedPath, out bytes);
            return bytes;
        }

        private void AddFileEntry(string inPackagePath, byte[] bytes)
        {
            var normalizedPath = inPackagePath.ToLowerInvariant();
            _files.Add(normalizedPath, bytes);
        }

        public void Save(ref FileStream stream)
        {
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                stream = null;

                // write files

                foreach (var file in _files)
                {
                    var fileName = Path.GetFileName(file.Key);
                    var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                        entryStream.Write(file.Value, 0, file.Value.Length);
                }

                // write metadata

                using (var metadataStream = ToXml(Metadata))
                {
                    var entry = archive.CreateEntry(MetadataFileName, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                        metadataStream.CopyTo(entryStream);
                }
            }
        }

        public static Package Load(Stream stream, long maxRawPkgSize)
        {
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
                PackageMetadata metadata = null;

                var rawSize = archive.Entries.Sum(e => e.Length);
                if (rawSize > maxRawPkgSize)
                    throw new Exception($"Invalid Algo package: Unpacked size({rawSize}) exceeds limit");

                foreach (var entry in archive.Entries)
                {
                    using (var entryStream = entry.Open())
                    {
                        var entryNormalizedPath = entry.FullName.ToLowerInvariant();

                        byte[] buffer = new byte[entry.Length];
                        using (MemoryStream bufferWrapper = new MemoryStream(buffer))
                        {
                            entryStream.CopyTo(bufferWrapper);

                            if (entryNormalizedPath == MetadataFileName)
                            {
                                bufferWrapper.Seek(0, SeekOrigin.Begin);
                                metadata = FromXml<PackageMetadata>(bufferWrapper);
                            }
                            else
                                files.Add(entryNormalizedPath, buffer);
                        }
                    }
                }

                if (metadata == null)
                    throw new Exception("Invalid Algo package: metadata file is missing.");

                return new Package(metadata, files);
            }
        }

        private static MemoryStream ToXml<T>(T obj)
        {
            var stream = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(T));

            var xmlSettings = new System.Xml.XmlWriterSettings
            {
                Indent = true
            };

            using (var xmlWriter = System.Xml.XmlWriter.Create(stream, xmlSettings))
                serializer.WriteObject(xmlWriter, obj);

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private static T FromXml<T>(MemoryStream stream)
        {
            var serializer = new DataContractSerializer(typeof(T));

            return (T)serializer.ReadObject(stream);
        }
    }
}
