using System;
using System.Collections.Generic;
using System.Text;

namespace ActorSharp
{
    internal class LocalPage<T> : List<T>
    {
        public bool Last { get; set; }
    }
}
