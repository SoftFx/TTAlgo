using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class UniqueNameGenerator : INameGenerator
    {
        private Dictionary<string, int> _dictNames = new Dictionary<string, int>();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string Generate()
        {
            throw new NotImplementedException();
        }

        public string GenerateFrom(string name)
        {
            try
            {
                EnsureExistenceOfkey(name);
                return _dictNames[name] == 0 ? name : $"{name} #{_dictNames[name]}";
            }
            finally
            {
                _dictNames[name]++;
            }
        }

        private void EnsureExistenceOfkey(string name)
        {
            if (!_dictNames.ContainsKey(name))
                _dictNames.Add(name, 0);
        }
    }

    internal interface INameGenerator
    {
        string GenerateFrom(string name);
        string Generate();
    }
}
