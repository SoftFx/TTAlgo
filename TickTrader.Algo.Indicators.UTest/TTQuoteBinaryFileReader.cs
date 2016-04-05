using System;
using System.Collections.Generic;
using System.IO;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.UTest
{
    public static class TTQuoteBinaryFileReader
    {
        public const long QuoteParamsSize = 48;

        public static readonly DateTime StartDate = DateTime.Parse("1970.01.01 00:00:00");

        public static List<Bar> ReadQuotes(string path)
        {
            var result = new List<Bar>();
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                if (file.Length%QuoteParamsSize != 0)
                {
                    throw new ArgumentException("Invalid file format.");
                }
                using (var reader = new BinaryReader(file))
                {
                    try
                    {
                        while (true)
                        {
                            var bar = new Bar
                            {
                                OpenTime = StartDate.Add(TimeSpan.FromSeconds(reader.ReadInt64())),
                                Open = reader.ReadDouble(),
                                High = reader.ReadDouble(),
                                Low = reader.ReadDouble(),
                                Close = reader.ReadDouble(),
                                Volume = reader.ReadInt64(),
                            };
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
