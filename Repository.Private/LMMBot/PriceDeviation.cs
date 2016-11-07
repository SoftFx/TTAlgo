using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMMBot
{
    internal class PriceDeviation
    {
        double MaxPriceDeviationOneSide { get; set; }
        double PreviousPrice { get; set; }
        double PreviousRandomizedPrice { get; set; }
        double Point { get; set; }
        Random r = new Random();

        public PriceDeviation(int maxPriceDeviationinPips, double point)
        {
            this.MaxPriceDeviationOneSide = (maxPriceDeviationinPips / 2)*point;
            this.Point = point;
        }

        public double ProcessPrice(double newPrice)
        {
            if (newPrice != PreviousPrice)
            {
                PreviousPrice = newPrice;
                PreviousRandomizedPrice = newPrice;
                return newPrice;
            }
            PreviousRandomizedPrice += (r.Next(3) - 1)*Point;
            if (PreviousRandomizedPrice - PreviousPrice > MaxPriceDeviationOneSide)
                PreviousRandomizedPrice = PreviousPrice + MaxPriceDeviationOneSide;
            else if(PreviousPrice - PreviousRandomizedPrice > MaxPriceDeviationOneSide)
                PreviousRandomizedPrice = PreviousPrice - MaxPriceDeviationOneSide;

            return PreviousRandomizedPrice;
        }
    }
}
