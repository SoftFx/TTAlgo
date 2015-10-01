using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Metadata
{
    internal class AlgoAssembly : IDisposable
    {
        //private Isolated<AlgoSandbox> isolatedSandbox = new Isolated<AlgoSandbox>();

        public AlgoAssembly(string filePath)
        {
            this.FilePath = filePath;
        }

        public string FilePath { get; private set; }

        public FileInfo FileInfo { get; private set; }

        public void Dispose()
        {
            //isolatedSandbox.Dispose();
        }

        internal void CheckForChanges()
        {
        }

        internal void Rename(string p)
        {
        }
    }
}
