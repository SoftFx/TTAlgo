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
                {
                    _accountInfo.OrderUpdated += PopupNotificationOnOrderUpdated;
                    _accountInfo.PositionUpdated += PopupNotificationOnPositionUpdated;
                }
                else
                {
                    _accountInfo.OrderUpdated -= PopupNotificationOnOrderUpdated;
                    _accountInfo.PositionUpdated -= PopupNotificationOnPositionUpdated;
                }

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
                    _accountInfo.PositionUpdated += SoundNotificationOnPositionUpdated;
                }
                else
                {
                    _connectionModel.Connected -= SoundNotificationOnConnected;
                    _accountInfo.OrderUpdated -= SoundNotificationOnOrderUpdated;
                    _accountInfo.PositionUpdated -= SoundNotificationOnPositionUpdated;
                }

                _notificationCenter.SoundNotification.Enabled = value;

                NotifyOfPropertyChange(nameof(SoundsEnabled));
            }
        }

        private void SoundNotificationOnPositionUpdated(PositionExecReport obj)
        {
            _notificationCenter.SoundNotification.Notify(AppSounds.Woosh);
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
            var message = NotificationBuilder.BuildMessage(obj);
            if (!message.IsEmpty)
                _notificationCenter.PopupNotification.Notify(new InfoMessage(message.Header, message.Body));
        }
        private void PopupNotificationOnPositionUpdated(PositionExecReport obj)
        {
            var message = NotificationBuilder.BuildMessage(obj);
            if (!message.IsEmpty)
                _notificationCenter.PopupNotification.Notify(new InfoMessage(message.Header, message.Body));
        }
    }

    public class NotificationBuilder
    {
        public class Message
        {
            public Message(string header, string body)
            {
                Header = header;
                Body = body;
            }

            public string Body { get; private set; }
            public string Header { get; private set; }
            public bool IsEmpty => string.IsNullOrWhiteSpace(Body) && string.IsNullOrWhiteSpace(Header);
        }

        public static Message BuildMessage(OrderExecReport obj)
        {
            var order = obj.OrderCopy;
            string header = "";
            string body = "";
            switch (obj.ExecAction)
            {
                case OrderExecAction.Opened:
                    switch (order.Type)
                    {
                        case OrderType.Position:
                            header = $"Order {obj.OrderId} Filled at {obj.OrderCopy.Price}";
                            body = $"Your request to {order.Side} {order.RequestedAmount} of {order.Symbol} was filled at {order.Price}.";
                            break;
                        case OrderType.Limit:
                        case OrderType.Stop:
                            header = $"Order {obj.OrderId} Placed at {order.Price}";
                            body = $"Your {order.Side} {order.Type} order for {order.RequestedAmount} of {order.Symbol} at {order.Price} was successfully placed.";
                            break;
                    }
                    break;
                case OrderExecAction.Modified:
                    switch (order.Type)
                    {
                        case OrderType.Position:
                            header = $"Order {obj.OrderId} Modified";
                            body = $"Order {order.Side} {order.RequestedAmount} of {order.Symbol} at {order.Price} modified.";
                            break;
                        case OrderType.Limit:
                        case OrderType.Stop:
                            header = $"Order {obj.OrderId} Modified";
                            body = $"Order {order.Side} {order.Type} {order.RequestedAmount} of {order.Symbol} at {order.Price} was successfully modified.";
                            break;
                    }
                    break;
                case OrderExecAction.Closed:
                    if (order.Type == Algo.Api.OrderType.Position)
                    {
                        header = $"Order {obj.OrderId} Filled at {order.Price}";
                        body = $"Your request to close position {order.Side} {order.RequestedAmount} of {order.Symbol} was filled at {order.Price}.";
                    }
                    break;
                case OrderExecAction.Canceled:
                    header = $"Order {obj.OrderId} Canceled";
                    body = $"Your order {order.Side} {order.Type} {order.RequestedAmount} of {order.Symbol} at {order.Price} was successfully canceled.";
                    break;
                case OrderExecAction.Filled:
                    header = $"Order {obj.OrderId} Filled at {order.Price}";
                    body = $"Your order {order.Side} {order.Type} {order.RequestedAmount} of {order.Symbol} was filled at at {order.Price}.";
                    break;
            }

            return new Message(header, body);
        }
        public static Message BuildMessage(PositionExecReport obj)
        {
            var position = obj.PositionCopy;
            string header = "";
            string body = "";
            switch (obj.ExecAction)
            {
                case OrderExecAction.Modified:
                    header = $"Position Modified";
                    body = $"Position {position.Side} {position.Amount} of {position.Symbol} at {position.Price} modified.";
                    break;
                case OrderExecAction.Closed:
                    header = $"Position Closed";
                    body = $"Position {position.Side} {position.Amount} of {position.Symbol} at {position.Price} closed.";
                    break;
            }
            return new Message(header, body);
        }
    }
}
