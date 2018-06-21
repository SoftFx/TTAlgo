using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api.Ext
{
    public interface DealerEmulator
    {
        DealerResponse ConfirmOrderOpen(Order order, RateUpdate rate);
    }

    //public interface DealerContext
    //{
    //    AccountDataProvider AccountData { get; }

    //    /// <summary>
    //    /// Returns current rate (last rate update) for specified symbol.
    //    /// </summary>
    //    /// <param name="symbol"></param>
    //    /// <returns></returns>
    //    RateUpdate GetRate(string symbol);

    //    /// <summary>
    //    /// Emulates delay.
    //    /// Note: Do not use time functions (such as Task.Delay() or Thread.Sleep()) in a dealer emulator. Use this function instead.
    //    /// </summary>
    //    /// <param name="delay"></param>
    //    /// <returns></returns>
    //    Task Delay(TimeSpan delay);
    //}

    public struct DealerResponse
    {
        /// <summary>
        /// Amount of transaction, confirmed by dealer. Can differ from requested amount in case of Market or Limit+IoC orders.
        /// Note: In most cases you won't modify amount for other order types (Limit without IoC, Stop, StopLimit)
        /// Set 0 to reject whole order.
        /// Keep null to confirm whole order.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Price, confirmed by dealer.
        /// </summary>
        public decimal? Price { get; set; }
    }

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DealerEmulatorAttribute : Attribute
    {
        public DealerEmulatorAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }
}
