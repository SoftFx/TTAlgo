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
            _notificationCenter.SoundNotification.Notify(AppSounds.Clinking);
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
                            header = string.Format("Order {0} Filled at {1}", obj.OrderId, obj.OrderCopy.Price);
                            body = string.Format("Your request to {0} {1} of {2} was filled at {3}.", obj.OrderCopy.Side, obj.OrderCopy.RequestedAmount, obj.OrderCopy.Symbol, obj.OrderCopy.Price);
                            break;
                        case OrderType.Limit:
                            header = string.Format("Order {0} Placed at {1} ", obj.OrderId, obj.OrderCopy.Price);
                            body = string.Format("Your {0} Limit order for {1} of {2} at {3} was successfully placed.", obj.OrderCopy.Side, obj.OrderCopy.RequestedAmount, obj.OrderCopy.Symbol, obj.OrderCopy.Price);
                            break;
                    }
                    break;
                case OrderExecAction.Modified:
                    if (obj.OrderCopy.Type == Algo.Api.OrderType.Limit)
                    {
                        header = string.Format("Order {0} Modified", obj.OrderId);
                        body = string.Format("The entry price of the order with ID {0}, was successfully modified.",obj.OrderCopy.Id);
                    }
                    break;
                case OrderExecAction.Closed:
                    if (obj.OrderCopy.Type == Algo.Api.OrderType.Position)
                    {
                        header = string.Format("Order {0} Filled at {1}", obj.OrderId, obj.OrderCopy.Price);
                        body = string.Format("Your request to close position {0} {1} of {2} was filled at {3}.", obj.OrderCopy.Side, obj.OrderCopy.RequestedAmount, obj.OrderCopy.Symbol, obj.OrderCopy.Price);
                    }
                    break;
                case OrderExecAction.Canceled:
                    header = string.Format("Order {0} Canceled", obj.OrderId);
                    body = string.Format("Your order {0} Limit {1} of {2} at {3} was successfully canceled.", obj.OrderCopy.Side, obj.OrderCopy.RequestedAmount, obj.OrderCopy.Symbol, obj.OrderCopy.Price);
                    break;
            }
            _notificationCenter.PopupNotification.Notify(new InfoMessage(header, body));
        }
    }
}
