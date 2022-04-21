using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Indicators.Tests.Utility
{
    public static class TTQuoteBinaryFileReader
    {
        public const long QuoteParamsSize = 48;

        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static List<BarData> ReadQuotes(string path)
        {
            var result = new List<BarData>();
            using (var file = File.OpenRead(path))
            {
                if (file.Length % QuoteParamsSize != 0)
                {
                    throw new ArgumentException("Invalid file format.");
                }
                using (var reader = new BinaryReader(file))
                {
                    try
                    {
                        while (true)
                        {
                            var time = new UtcTicks(UnixEpoch.AddSeconds(reader.ReadInt64()));
                            var bar = new BarData(time, time)
                            {
                                Open = reader.ReadDouble(),
                                High = reader.ReadDouble(),
                                Low = reader.ReadDouble(),
                                Close = reader.ReadDouble(),
                                RealVolume = reader.ReadInt64(),
                            };
                            bar.TickVolume = (long)bar.RealVolume;
                            result.Add(bar);

                        }
                    }
                    catch (EndOfStreamException)
                    {
                    }
                }
            }
            return result;
        }
    }
}
