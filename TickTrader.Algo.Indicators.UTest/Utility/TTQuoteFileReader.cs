using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest.Utility
{
    class TTQuoteFileReader
    {
        public static List<BarEntity> ReadFile(string path)
        {
            using (var file = System.IO.File.OpenText(path))
            {
                List<BarEntity> result = new List<BarEntity>();

                while (true)
                {
                    BarEntity nextBar;
                    if (!ReadNext(out nextBar, file))
                        break;
                    result.Add(nextBar);
                }

                return result;
            }
        }

        private static bool ReadNext(out BarEntity bar, System.IO.StreamReader reader)
        {
            bar = new BarEntity();

            string line = reader.ReadLine();
            if (line == null)
                return false;

            string[] parts = line.Split('\t');

            if (parts.Length != 6)
                throw new System.IO.InvalidDataException("Invalid file format!");

            try
            {
                bar.OpenTime = DateTime.Parse(parts[0]);
                bar.Open = double.Parse(parts[1]);
                bar.High = double.Parse(parts[2]);
                bar.Low = double.Parse(parts[3]);
                bar.Close = double.Parse(parts[4]);
                bar.Volume = double.Parse(parts[5]);
            }
            catch (Exception)
            {
                throw new System.IO.InvalidDataException("Invalid file format!");
            }

            return true;
        }
    }
}
