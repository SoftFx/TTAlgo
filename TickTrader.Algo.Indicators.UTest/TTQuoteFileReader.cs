using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Indicators.UTest
{
    public class TTQuoteFileReader : List<BarEntity>
    {
        private System.IO.StreamReader reader;

        public TTQuoteFileReader(System.IO.StreamReader reader)
        {
            this.reader = reader;

            while (true)
            {
                BarEntity nextbar;
                if (!ReadNext(out nextbar))
                    break;
                Add(nextbar);
            }
        }

        private bool ReadNext(out BarEntity bar)
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
