using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreUsageSample
{
    class TTQuoteFileReader
    {
        public static List<BarData> ReadFile(string path)
        {
            using (var file = System.IO.File.OpenText(path))
            {
                var result = new List<BarData>();

                while (true)
                {
                    if (!ReadNext(out var nextBar, file))
                        break;
                    result.Add(nextBar);
                }

                return result;
            }
        }

        private static bool ReadNext(out BarData bar, System.IO.StreamReader reader)
        {
            bar = new BarData();

            var line = reader.ReadLine();
            if (line == null)
                return false;

            var parts = line.Split('\t');

            if (parts.Length != 6)
                throw new System.IO.InvalidDataException("Invalid file format!");

            try
            {
                bar.OpenTime = DateTime.Parse(parts[0]).ToTimestamp();
                bar.Open = double.Parse(parts[1]);
                bar.High = double.Parse(parts[2]);
                bar.Low = double.Parse(parts[3]);
                bar.Close = double.Parse(parts[4]);
                bar.RealVolume = double.Parse(parts[5]);
            }
            catch (Exception)
            {
                throw new System.IO.InvalidDataException("Invalid file format!");
            }

            return true;
        }
    }
}
