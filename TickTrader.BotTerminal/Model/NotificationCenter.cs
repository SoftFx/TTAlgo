using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Toaster;
using TickTrader.Toaster.Messages;

namespace TickTrader.BotTerminal
{
    internal interface INotification<T>
    {
        bool Enabled { get; set; }
        void Notify(T notification);
    }

    internal class PopupNotification : INotification<BaseMessage>
    {
        public bool Enabled { get; set; }

        public void Notify(BaseMessage message)
        {
            if (Enabled)
                Toast.Pop(message);
        }
    }

    internal class SoundNotification : INotification<IPlayable>
    {
        public bool Enabled { get; set; }

        public void Notify(IPlayable sound)
        {
            if (Enabled)
                sound.Play();
        }
    }

    internal interface INotificationCenter
    {
        INotification<BaseMessage> PopupNotification { get; }
        INotification<IPlayable> SoundNotification { get; }
    }

    internal class NotificationCenter : INotificationCenter
    {
        public NotificationCenter(INotification<BaseMessage> popupNotification, INotification<IPlayable> soundNotification)
        {
            this.PopupNotification = popupNotification;
            this.SoundNotification = soundNotification;
        }

        public INotification<BaseMessage> PopupNotification { get; private set; }
        public INotification<IPlayable> SoundNotification { get; private set; }
    }
}
