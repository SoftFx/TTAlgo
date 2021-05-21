using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface File
    {
        string FullPath { get; }
        bool IsNull { get; }

        string ReadAllText();
        byte[] ReadAllBytes();
        FileStream Open(FileMode mode);
    }
}
