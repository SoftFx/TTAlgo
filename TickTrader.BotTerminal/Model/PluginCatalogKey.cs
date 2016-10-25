using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class PluginCatalogKey : IComparable
    {
        private int hash;
        private string compositeField;

        public PluginCatalogKey(Guid repositoryId, string fileName, string className)
        {
            this.RepositoryId = repositoryId;
            this.FileName = fileName;
            this.ClassName = className;

            compositeField = repositoryId.ToString() + System.IO.Path.Combine(fileName + className);

            hash = compositeField.GetHashCode();

            //hash = 269;
            //hash = (hash * 47) + RepositoryId.GetHashCode();
            //hash = (hash * 47) + FileName.GetHashCode();
            //hash = (hash * 47) + ClassName.GetHashCode();
        }

        public Guid RepositoryId { get; private set; }
        public string FileName { get; private set; }
        public string ClassName { get; private set; }

        public override bool Equals(object obj)
        {
            var key = obj as PluginCatalogKey;
            return key != null
                && key.RepositoryId == RepositoryId
                && key.FileName == FileName
                && key.ClassName == ClassName;
        }

        public override string ToString()
        {
            return compositeField;
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public int CompareTo(object obj)
        {
            var key = (PluginCatalogKey)obj;
            return compositeField.CompareTo(key.compositeField);
        }
    }
}
