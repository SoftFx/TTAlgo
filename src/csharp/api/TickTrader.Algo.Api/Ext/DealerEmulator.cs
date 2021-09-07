using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api.Ext
{
    public interface DealerEmulator
    {
        void ConfirmOrderOpen(OpenOrderRequest request);
        void ConfirmOrderCancelation(CancelOrderRequest request);
        void ConfirmOrderReplace(ModifyOrderRequest request);
        void ConfirmPositionClose(ClosePositionRequest request);
    }

    public interface OpenOrderRequest
    {
        Order Order { get; }
        Quote CurrentRate { get; }

        void Confirm();
        void Confirm(double amount, double price);
        void Reject();
    }

    public interface CancelOrderRequest
    {
        Order Order { get; }
        Quote CurrentRate { get; }

        void Confirm();
        void Reject();
    }

    public interface ModifyOrderRequest
    {
        double? NewVolume { get; }
        double? NewPrice { get; }
        double? NewStopPrice { get; }
        string NewComment { get; }

        Order Order { get; }
        Quote CurrentRate { get; }

        void Confirm();
        void Reject();
    }

    public interface ClosePositionRequest
    {
        double? CloseAmount { get; }

        Order Position { get; }
        Quote CurrentRate { get; }

        void Confirm();
        void Confirm(double price);
        void Reject();
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
