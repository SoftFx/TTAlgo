using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class AlertsManager
    {
        private VarDictionary<string, AlertViewModel> _alerts = new VarDictionary<string, AlertViewModel>();

        private WindowManager _wnd;

        internal AlertsManager(WindowManager wnd)
        {
            _wnd = wnd;
        }

        public void AddMessage(string key, string message)
        {
            var alert = _alerts.GetOrDefault(key) ?? new AlertViewModel(key);

            _wnd.OpenMdiWindow(alert);

            if (!_alerts.ContainsKey(key))
                _alerts.Add(key, alert);
        }
    }
}
