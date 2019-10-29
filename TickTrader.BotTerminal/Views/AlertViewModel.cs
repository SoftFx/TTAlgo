using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class AlertViewModel : Screen, IWindowModel
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int MaxBufferSize = 100;

        public ObservableCircularList<IAlertUpdateEventArgs> AlertBuffer { get; }

        private WindowManager _wnd;

        internal AlertViewModel(WindowManager wnd)
        {
            _wnd = wnd;

            AlertBuffer = new ObservableCircularList<IAlertUpdateEventArgs>();
            DisplayName = "Alert";
        }

        public void UpdateAlertModel(IAlertUpdateEventArgs args)
        {
            _wnd.OpenMdiWindow(this);

            switch (args.Type)
            {
                case AlertEventType.Update:
                    AddRecord(args);
                    break;
                default:
                    AlertBuffer.Clear();
                    break;
            }
        }

        public void Clear()
        {
            AlertBuffer.Clear();
        }

        public void Ok()
        {
            TryClose();
        }

        private void AddRecord(IAlertUpdateEventArgs record)
        {
            while (AlertBuffer.Count > MaxBufferSize)
                AlertBuffer.Dequeue();

            AlertBuffer.Add(record);
        }
    }


    public class AlertUpdateEventArgsImpl : IAlertUpdateEventArgs
    {
        public DateTime Time { get; }

        public string InstanceId { get; }

        public string Message { get; }

        public AlertEventType Type { get; }


        public AlertUpdateEventArgsImpl(string id, AlertEventType type, string message = "")
        {
            Time = DateTime.Now;
            InstanceId = id;
            Message = message;
            Type = type;
        }
    }

    public interface IAlertUpdateEventArgs
    {
        DateTime Time { get; }

        string InstanceId { get; }

        string Message { get; }

        AlertEventType Type { get; }
    }

    public enum AlertEventType { Update, Clear }
}
