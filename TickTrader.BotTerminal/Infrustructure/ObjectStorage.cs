using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal interface IObjectStorage
    {
        void Save<T>(string fileName, T obj);

        T Load<T>(string fileName);
    }


    internal interface IBinStorage
    {
        void Save(string fileName, MemoryStream binData);

        MemoryStream LoadData(string fileName);
    }


    internal class XmlObjectStorage : IObjectStorage
    {
        private IBinStorage _binaryStorage;


        public XmlObjectStorage(IBinStorage binaryStorage)
        {
            this._binaryStorage = binaryStorage;
        }


        public T Load<T>(string fileName)
        {
            using (var stream = _binaryStorage.LoadData(fileName))
            {
                if (stream.Length == 0)
                    return default;

                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

        public void Save<T>(string fileName, T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
#if DEBUG
                using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
                {
                    serializer.WriteObject(xmlWriter, obj);
                }
#else
                serializer.WriteObject(stream, obj);
#endif
                _binaryStorage.Save(fileName, stream);
            }
        }
    }


    internal class JsonObjectStorage : IObjectStorage
    {
        private static readonly JsonSerializer _serializer =
            JsonSerializer.Create(new JsonSerializerSettings { Formatting = Newtonsoft.Json.Formatting.Indented });

        private IBinStorage _binaryStorage;


        public JsonObjectStorage(IBinStorage binaryStorage)
        {
            _binaryStorage = binaryStorage;
        }


        public T Load<T>(string fileName)
        {
            using (var stream = _binaryStorage.LoadData(fileName))
            {
                if (stream.Length == 0)
                    return default;

                using var reader = new StreamReader(stream);
                return (T)_serializer.Deserialize(reader, typeof(T));
            }
        }

        public void Save<T>(string fileName, T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, leaveOpen: true))
                {
                    _serializer.Serialize(writer, obj);
                }

                _binaryStorage.Save(fileName, stream);
            }
        }
    }


    internal class FolderBinStorage : IBinStorage
    {
        private string _folder;

        public FolderBinStorage(string folder)
        {
            this._folder = folder;
        }

        public MemoryStream LoadData(string fileName)
        {
            string filePath = Path.Combine(_folder, fileName);
            if (!File.Exists(filePath))
                return new MemoryStream();

            var data = File.ReadAllBytes(filePath);
            return new MemoryStream(data);
        }

        public void Save(string fileName, MemoryStream binData)
        {
            PathHelper.EnsureDirectoryCreated(_folder);
            string filePath = Path.Combine(_folder, fileName);
            using (var file = File.Open(filePath, FileMode.Create))
                binData.WriteTo(file);
        }
    }


    internal class SecureStorageLayer : IBinStorage
    {
        private IBinStorage _binaryStorage;
        private byte[] _entropy;
        private DataProtectionScope _scope;


        public SecureStorageLayer(IBinStorage binaryStorage, byte[] entropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            this._binaryStorage = binaryStorage;
            this._entropy = entropy;
            this._scope = scope;
        }


        public MemoryStream LoadData(string fileName)
        {
            using (var encryptedStream = _binaryStorage.LoadData(fileName))
            {
                if (encryptedStream.Length == 0)
                    return encryptedStream;

                var encryptedData = encryptedStream.ToArray();
                var unencryptedData = ProtectedData.Unprotect(encryptedData, _entropy, _scope);
                return new MemoryStream(unencryptedData);
            }
        }

        public void Save(string fileName, MemoryStream binData)
        {
            var encryptedData = ProtectedData.Protect(binData.ToArray(), _entropy, _scope);
            using (MemoryStream memStream = new MemoryStream(encryptedData))
                _binaryStorage.Save(fileName, memStream);
        }
    }


    public class StorageException : Exception
    {
        public StorageException(string msg, Exception innerException = null)
            : base(msg, innerException)
        {
        }
    }
}
