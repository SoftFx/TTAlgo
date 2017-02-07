using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Repository
{
    public class RepositoryKey
    {
        private int hash;

        public RepositoryKey(Guid repositoryId, string fileName, string className)
        {
            this.RepositoryId = repositoryId;
            this.FileName = fileName;
            this.ClassName = className;

            hash = 269;
            hash = (hash * 47) + RepositoryId.GetHashCode();
            hash = (hash * 47) + FileName.GetHashCode();
            hash = (hash * 47) + ClassName.GetHashCode();
        }

        public Guid RepositoryId { get; private set; }
        public string FileName { get; private set; }
        public string ClassName { get; private set; }

        public override bool Equals(object obj)
        {
            var key = obj as RepositoryKey;
            return key != null
                && key.RepositoryId == RepositoryId
                && key.FileName == FileName
                && key.ClassName == ClassName;
        }

        public override int GetHashCode()
        {
            return hash;
        }
    }
}
