using System;
using System.IO;

namespace TickTrader.Algo.Core.Lib.FileStreams
{
    public sealed class CulturalStreamWriter : StreamWriter
    {
        private readonly IFormatProvider _provider;


        public override IFormatProvider FormatProvider => _provider;


        public CulturalStreamWriter(Stream stream, IFormatProvider provider) : base(stream)
        {
            _provider = provider;
        }
    }
}
