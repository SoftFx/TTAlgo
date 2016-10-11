using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot
{
    internal class Entry
    {
        public double price;
        public double volume;
        public override string ToString()
        {
            return price.ToString() + " " + volume.ToString();
        }
    }
    internal class BookSide
    {
        public List<Entry> listEntries = new List<Entry>();
        public BookSide(IEnumerable<BookEntry> bookEntries, bool reverse = false)
        {
            foreach (BookEntry e in bookEntries)
            {
                if (reverse)
                    listEntries.Add(new Entry { price = 1/e.Price, volume = e.Volume*e.Price });
                else
                    listEntries.Add(new Entry { price = e.Price, volume = e.Volume });

            }
        }
        /// <summary>
        /// Return vwap price for requested volume or if it is negatice coef to reduce requested volume
        /// </summary>
        /// <param name="requiredVolume"></param>
        /// <returns></returns>
        public double GetPriceForVolume(double requiredVolume)
        {
            double leftVolume = requiredVolume;
            double numerator = 0;
            foreach( Entry currEntry in listEntries)
            {
                if(leftVolume  >= currEntry.volume)
                {
                    leftVolume -= currEntry.volume;
                    numerator += currEntry.volume * currEntry.price;
                }
                else
                {
                    numerator += leftVolume * currEntry.price;
                    leftVolume = 0;
                }
            }
            if (leftVolume > float.Epsilon)
                return leftVolume / requiredVolume-1;
            return numerator / requiredVolume;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            foreach (Entry entry in listEntries)
                s.Append(entry.ToString() + "|");
            return s.ToString();
        }
    }
    internal class DirectEdge
    {
        public string From { get; private set; }
        public string To { get; private set; }
        public BookSide Weight { get; private set; }

        public DirectEdge(string from, string to, BookSide weight)
        {
            this.From = from;
            this.To = to;
            this.Weight = weight;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}={2}", From, To, Weight.ToString());
        }

    }
    internal class EdgeWeightedDigraph
    {
        private Dictionary<string, Dictionary<string, DirectEdge>> Vertices = new Dictionary<string, Dictionary<string, DirectEdge>>();
        public EdgeWeightedDigraph(IEnumerable<string> vertices)
        {
            foreach (string currV in vertices)
                this.Vertices.Add(currV, new Dictionary<string, DirectEdge>());
        }

        public DirectEdge GetEdge(string from, string to)
        {
            try
            {
                return Vertices[from][to];
            }
            catch(Exception exc)
            {
                throw new ApplicationException(string.Format("There is no {0}{1} symbol.", from, to));
            }

        }

        public void AddEdge(DirectEdge edge)
        {
            Vertices[edge.From][edge.To] = edge;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine("Vertices = " + String.Join<string>(" ", Vertices.Keys));
            foreach (Dictionary<string, DirectEdge> a in Vertices.Values)
                foreach (DirectEdge b in a.Values)
                    s.AppendLine(b.ToString());
            return s.ToString();
        }
    }
}
