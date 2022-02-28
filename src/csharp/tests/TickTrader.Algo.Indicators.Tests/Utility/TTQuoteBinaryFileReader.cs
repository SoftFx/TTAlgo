using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Indicators.Tests.Utility
{
    public static class TTQuoteBinaryFileReader
    {
        public const long QuoteParamsSize = 48;

        public static readonly DateTime StartDate = DateTime.Parse("1970.01.01 00:00:00");

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
                            var bar = new BarData
                            {
                                OpenTime = TimeMs.FromTimestamp(new Timestamp {Seconds = reader.ReadInt64() }),
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
