using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        private IBinStorage binaryStorage;

        public XmlObjectStorage(IBinStorage binaryStorage)
        {
            this.binaryStorage = binaryStorage;
        }

        public T Load<T>(string fileName)
        {
            using (var stream = binaryStorage.LoadData(fileName))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

        public void Save<T>(string fileName, T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(stream, obj);
                binaryStorage.Save(fileName, stream);
            }
        }
    }

    internal class FolderBinStorage : IBinStorage
    {
        private string folder;

        public FolderBinStorage(string folder)
        {
            this.folder = folder;
        }

        public MemoryStream LoadData(string fileName)
        {
            string filePath = Path.Combine(folder, fileName);
            var data = File.ReadAllBytes(filePath);
            return new MemoryStream(data);
        }

        public void Save(string fileName, MemoryStream binData)
        {
            string filePath = Path.Combine(folder, fileName);
            using (var file = File.Open(filePath, FileMode.Create))
                binData.WriteTo(file);
        }
    }

    internal class SecureStorageLayer : IBinStorage
    {
        private IBinStorage binaryStorage;
        private byte[] entropy;
        private DataProtectionScope scope;

        public SecureStorageLayer(IBinStorage binaryStorage, byte[] entropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            this.binaryStorage = binaryStorage;
            this.entropy = entropy;
            this.scope = scope;
        }

        public MemoryStream LoadData(string fileName)
        {
            using (var encryptedStream = binaryStorage.LoadData(fileName))
            {
                var encryptedData = encryptedStream.ToArray();
                var unencryptedData = ProtectedData.Unprotect(encryptedData, entropy, scope);
                return new MemoryStream(unencryptedData);
            }
        }

        public void Save(string fileName, MemoryStream binData)
        {
            var encryptedData = ProtectedData.Protect(binData.ToArray(), entropy, scope);
            using (MemoryStream memStream = new MemoryStream(encryptedData))
                binaryStorage.Save(fileName, memStream);
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
