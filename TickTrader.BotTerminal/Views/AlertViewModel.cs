using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class AlertViewModel : Screen, IWindowModel
    {
        //VarContext _var = new VarContext();

        private const int MaxBufferSize = 100;

        private LinkedList<string> _buffer = new LinkedList<string>();

        internal AlertViewModel(string id)
        {
            Id = id;
            DisplayName = $"Alerts: {Id}";
        }

        public string Id { get; }

        public void AddMessage(string message)
        {
            while (_buffer.Count >= MaxBufferSize)
                _buffer.RemoveFirst();

            _buffer.AddLast(message);
        }
    }
}
