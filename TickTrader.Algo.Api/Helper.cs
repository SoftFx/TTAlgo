using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public static class Helper
    {
        #region Math

        /// <summary>
        /// Rounds a volume to nearest allowed by this symbol. Resulting volume is less or equal specified.
        /// Returns 0 in case the specified volume is less than allowed minimum.
        /// </summary>
        /// <param name="volume">Unrounded volume</param>
        /// <param name="symbolInfo">target symbol metadata</param>
        /// <returns>
        /// Rounded volume or zero.
        /// </returns>
        public static double RoundVolumeDown(double volume, Symbol symbolInfo)
        {
            return AlgoPlugin.Context.Helper.RoundVolumeDown(volume, symbolInfo);
        }

        /// <summary>
        /// Rounds a volume to nearest allowed by this symbol. Resulting volume is more or equal specified.
        /// Returns minimal trade volume in case the specified volume is less than allowed minimum.
        /// </summary>
        /// <param name="volume">Unrounded volume</param>
        /// <param name="symbolInfo">target symbol metadata</param>
        /// <returns>
        /// Rounded volume
        /// </returns>
        public static double RoundVolumeUp(double volume, Symbol symbolInfo)
        {
            return AlgoPlugin.Context.Helper.RoundVolumeUp(volume, symbolInfo);
        }

        #endregion

        #region Format

        public static string FormatPrice(double price, int digits)
        {
            return AlgoPlugin.Context.Helper.FormatPrice(price, digits);
        }

        public static string FormatPrice(double price, Symbol symbolInfo)
        {
            return AlgoPlugin.Context.Helper.FormatPrice(price, symbolInfo);
        }

        #endregion

        #region Entities Constructors

        public static Quote CreateQuote(string symbol, DateTime time, IEnumerable<BookEntry> bids, IEnumerable<BookEntry> asks)
        {
            return AlgoPlugin.Context.Helper.CreateQuote(symbol, time, bids, asks);
        }

        public static BookEntry CreateBookEntry(double price, double volume)
        {
            return AlgoPlugin.Context.Helper.CreateBookEntry(price, volume);
        }

        public static IEnumerable<BookEntry> CreateBook(IEnumerable<double> prices, IEnumerable<double> volumes)
        {
            return AlgoPlugin.Context.Helper.CreateBook(prices, volumes);
        }

        #endregion
    }

    public static class HelperExtensions
    {
        public static string FormatPrice(this Symbol symbolInfo, double price)
        {
            return AlgoPlugin.Context.Helper.FormatPrice(price, symbolInfo);
        }

        public static double RoundPriceDown(this Symbol symbolInfo, double price)
        {
            return AlgoPlugin.Context.Helper.RoundVolumeDown(price, symbolInfo);
        }

        public static double RoundPriceUp(this Symbol symbolInfo, double price)
        {
            return AlgoPlugin.Context.Helper.RoundVolumeUp(price, symbolInfo);
        }
    }
}
