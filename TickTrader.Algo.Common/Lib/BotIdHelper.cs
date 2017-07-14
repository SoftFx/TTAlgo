using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TickTrader.Algo.Common.Lib
{
    public class BotIdHelper
    {
        private string _antiPattern = "[^A-Za-z0-9 ]";
        private int _maxLength = 30;

        public int MaxLength => _maxLength;
        public string Pattern => _antiPattern;

        public void UseMaxLength(int idLength)
        {
            _maxLength = idLength;
        }

        public void ExcludeCharacters(string pattern)
        {
            _antiPattern = pattern;
        }

        public string BuildId(string botName, string suffix)
        {
            var botIdBulder = new StringBuilder(Regex.Replace(botName, _antiPattern, ""));
            botIdBulder.Length -= Math.Max(0, botIdBulder.Length + suffix.Length + 1 - _maxLength);
            botIdBulder.Append(" ").Append(suffix);
            
            return botIdBulder.ToString();
        }

        public bool Validate(string botId)
        {
            return botId.Length <= _maxLength && !Regex.IsMatch(botId, _antiPattern);
        }
    }
}
