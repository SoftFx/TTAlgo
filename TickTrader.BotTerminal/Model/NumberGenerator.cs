using System;

namespace TickTrader.BotTerminal
{
    public class IndificationNumberGenerator
    {
        private DateTime _lastDate = DateTime.MinValue;

        private int _lastNumber;

        public string GetNumber(DateTime current)
        {
            _lastNumber = DateTime.Compare(_lastDate, current) == 0 ? _lastNumber + 1 : 1;
            _lastDate = current;

            return _lastNumber.ToString();
        }
    }
}
