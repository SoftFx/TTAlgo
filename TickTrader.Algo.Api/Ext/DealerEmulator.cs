using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api.Ext
{
    public interface DealerEmulator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="order">Order ro confirm.</param>
        /// <param name="rate">Current rate for corresponding symbol.</param>
        /// <param name="fill">Immediate fill data. For Limit+IoC and Market orders only. Default values are used in case of null.</param>
        /// <returns>True to confirm request, false to reject.</returns>
        bool ConfirmOrderOpen(Order order, RateUpdate rate, out FillInfo? fill);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order">Order to cancel.</param>
        /// <returns>True to confirm request, false to reject.</returns>
        bool ConfirmOrderCancelation(Order order);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order">Order to modify.</param>
        /// <param name="request">New values.</param>
        /// <returns>True to confirm request, false to reject.</returns>
        bool ConfirmOrderReplace(Order order, OrderModifyInfo request);
    }

    public interface OrderModifyInfo
    {
        double? NewVolume { get; }
        double? NewPrice { get; }
        double? NewStopPrice { get; }
        string NewComment { get; }
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

    public struct FillInfo
    {
        public FillInfo(decimal amount, decimal price)
        {
            ExecAmount = amount;
            ExecPrice = price;
        }

        public decimal ExecAmount { get; set; }
        public decimal ExecPrice { get; set; }
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
