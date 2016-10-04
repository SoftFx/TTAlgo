using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Toaster.Messages;

namespace TickTrader.BotTerminal
{
    internal class NotificationsViewModel : PropertyChangedBase
    {
        private IAccountInfoProvider _accountInfo;
        private ConnectionModel _connectionModel;
        private INotificationCenter _notificationCenter;

        public NotificationsViewModel(INotificationCenter notificationCenter, IAccountInfoProvider accountInfo, ConnectionModel connectionModel)
        {
            _accountInfo = accountInfo;
            _connectionModel = connectionModel;
            _notificationCenter = notificationCenter;

            NotificationsEnabled = true;
            SoundsEnabled = true;
        }

        public bool NotificationsEnabled
        {
            get
            {
                return _notificationCenter.PopupNotification.Enabled;
            }
            private set
            {
                if (_notificationCenter.PopupNotification.Enabled == value)
                    return;

                if (value)
                    _accountInfo.OrderUpdated += PopupNotificationOnOrderUpdated;
                else
                    _accountInfo.OrderUpdated -= PopupNotificationOnOrderUpdated;

                _notificationCenter.PopupNotification.Enabled = value;

                NotifyOfPropertyChange(nameof(NotificationsEnabled));
            }
        }
        public bool SoundsEnabled
        {
            get
            {
                return _notificationCenter.SoundNotification.Enabled;
            }
            private set
            {
                if (_notificationCenter.SoundNotification.Enabled == value)
                    return;

                if (value)
                {
                    _connectionModel.Connected += SoundNotificationOnConnected;
                    _accountInfo.OrderUpdated += SoundNotificationOnOrderUpdated;
                }
                else
                {
                    _connectionModel.Connected -= SoundNotificationOnConnected;
                    _accountInfo.OrderUpdated -= SoundNotificationOnOrderUpdated;
                }

                _notificationCenter.SoundNotification.Enabled = value;

                NotifyOfPropertyChange(nameof(SoundsEnabled));
            }
        }

        private void SoundNotificationOnOrderUpdated(OrderExecReport obj)
        {
            _notificationCenter.SoundNotification.Notify(AppSounds.Woosh);
        }
        private void SoundNotificationOnConnected()
        {
            _notificationCenter.SoundNotification.Notify(AppSounds.Positive);
        }
        private void PopupNotificationOnOrderUpdated(OrderExecReport obj)
        {
            string header = "";
            string body = "";
            switch (obj.ExecAction)
            {
                case OrderExecAction.Opened:
                    switch (obj.OrderCopy.Type)
                    {
                        case OrderType.Position:
                            header = $"Order {obj.OrderId} Filled at {obj.OrderCopy.Price}";
                            body = $"Your request to {obj.OrderCopy.Side} {obj.OrderCopy.RequestedAmount} of {obj.OrderCopy.Symbol} was filled at {obj.OrderCopy.Price}.";
                            break;
                        case OrderType.Limit:
                        case OrderType.Stop:
                            header = $"Order {obj.OrderId} Placed at {obj.OrderCopy.Price}";
                            body =$"Your {obj.OrderCopy.Side} {obj.OrderCopy.Type} order for {obj.OrderCopy.RequestedAmount} of {obj.OrderCopy.Symbol} at {obj.OrderCopy.Price} was successfully placed.";
                            break;
                    }
                    break;
                case OrderExecAction.Modified:
                    switch(obj.OrderCopy.Type)
                    {
                        case OrderType.Position:
                            header = $"Position {obj.OrderId} Modified";
                            body = $"Position {obj.OrderCopy.Side} {obj.OrderCopy.RequestedAmount} of {obj.OrderCopy.Symbol} at {obj.OrderCopy.Price} modified.";
                            break;
                        case OrderType.Limit:
                        case OrderType.Stop:
                            header = $"Order {obj.OrderId} Modified";
                            body = $"Order {obj.OrderCopy.Side} {obj.OrderCopy.Type} {obj.OrderCopy.RequestedAmount} of {obj.OrderCopy.Symbol} at {obj.OrderCopy.Price} was successfully modified.";
                            break;
                    }
                    break;
                case OrderExecAction.Closed:
                    if (obj.OrderCopy.Type == Algo.Api.OrderType.Position)
                    {
                        header = $"Order {obj.OrderId} Filled at {obj.OrderCopy.Price}";
                        body = $"Your request to close position {obj.OrderCopy.Side} {obj.OrderCopy.RequestedAmount} of {obj.OrderCopy.Symbol} was filled at {obj.OrderCopy.Price}.";
                    }
                    break;
                case OrderExecAction.Canceled:
                    header = $"Order {obj.OrderId} Canceled";
                    body = $"Your order {obj.OrderCopy.Side} {obj.OrderCopy.Type} {obj.OrderCopy.RequestedAmount} of {obj.OrderCopy.Symbol} at {obj.OrderCopy.Price} was successfully canceled.";
                    break;
            }
            _notificationCenter.PopupNotification.Notify(new InfoMessage(header, body));
        }
    }
}
