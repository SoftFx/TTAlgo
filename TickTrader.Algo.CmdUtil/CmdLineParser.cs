using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.CmdUtil
{
    public class CmdLineParser
    {
        private Dictionary<string, IOptionParser> options = new Dictionary<string, IOptionParser>();
        private int paramCount;
        private Action<int, string> paramHandler;
        private string[] optionPrefixes;
        private string[] optionDelimiters;

        public CmdLineParser(string optionPrefix = "/|-", string optionValueDelimiter = "=")
        {
            if (string.IsNullOrEmpty(optionPrefix))
                optionPrefixes = null;
            else
            {
                optionPrefixes = optionPrefix.Split('|');
                ValidateAliases(optionPrefixes);
            }
            optionDelimiters = optionValueDelimiter.Split('|');
            ValidateAliases(optionDelimiters);
        }

        public void OnSwitch(string optionName, Action optionHandler)
        {
            if (optionHandler == null)
                throw new ArgumentNullException("optionHandler");

            AddOptionHandler(optionName, () => new Switch(optionHandler));
        }

        public void OnOption(string optionName, Action<string> optionHandler)
        {
            if (optionHandler == null)
                throw new ArgumentNullException("optionHandler");

            AddOptionHandler(optionName, () => new Option(optionHandler));
        }

        public void OnRepeatableOption(string optionName, Action<int, string> optionHandler)
        {
            if (optionHandler == null)
                throw new ArgumentNullException("optionHandler");

            AddOptionHandler(optionName, () => new RepeatableOption(optionHandler));
        }

        private void AddOptionHandler(string optionName, Func<IOptionParser> factory)
        {
            if (string.IsNullOrWhiteSpace(optionName))
                throw new ArgumentException("Option name cannot be empty!");

            string[] aliases = optionName.Split('|');
            ValidateAliases(aliases);

            foreach (var alias in aliases)
            {
                if (options.ContainsKey(alias))
                    throw new ArgumentException("Name/alias has been already taken: " + alias);
            }

            var option = factory();
            foreach (var alias in aliases)
                options[alias] = option;
        }

        public void Parse(string[] cmdLine)
        {
            foreach (var cmd in cmdLine)
                Parse(cmd);
        }

        private void Parse(string cmdParam)
        {
            string name = null;
            string value = null;

            if (optionPrefixes == null)
            {
                foreach (var delimiter in optionDelimiters)
                {
                    int delimiterIndex = cmdParam.IndexOf(delimiter);
                    if (delimiterIndex >= 0)
                    {
                        value = cmdParam.Substring(delimiterIndex + 1);
                        name = cmdParam.Substring(0, delimiterIndex);
                        break;
                    }
                }
            }
            else
            {
                string prefix = optionPrefixes.FirstOrDefault(p => cmdParam.StartsWith(p));
                name = cmdParam.Substring(prefix.Length);

                foreach (var delimiter in optionDelimiters)
                {
                    int delimiterIndex = name.IndexOf(delimiter);
                    if (delimiterIndex < 0)
                        continue;
                    value = name.Substring(delimiterIndex + 1);
                    name = name.Substring(0, delimiterIndex);
                }
            }

            if (name != null)
            {
                IOptionParser optionParser;
                if (!options.TryGetValue(name, out optionParser))
                    throw new CmdLineParseException("Unknown or invalid option: " + cmdParam);
                optionParser.OnOption(name, value);
                return;
            }
            else
            {
                paramHandler?.Invoke(paramCount, cmdParam);
                paramCount++;
            }
        }

        private void ValidateAliases(string[] aliases)
        {
            foreach (var alias in aliases)
            {
                if (string.IsNullOrWhiteSpace(alias))
                    throw new Exception("Empty aliases are not allowed!");
            }
        }
        
        public void OnParameter(Action<int, string> paramHandler)
        {
            if (paramHandler == null)
                throw new ArgumentNullException("paramHandler");
            if (this.paramHandler != null)
                throw new InvalidOperationException("You cannot set parameter handler twice!");
            this.paramHandler = paramHandler;
        }

        private interface IOptionParser
        {
            void OnOption(string name, string value);
        }

        private class Switch : IOptionParser
        {
            private int count;
            private Action optionHandler;

            public Switch(Action optionHandler)
            {
                this.optionHandler = optionHandler;
            }

            public void OnOption(string name, string value)
            {
                if (count > 0)
                    throw new CmdLineParseException("Option cannot be set multiple times: " + name);
                if (value != null)
                    throw new CmdLineParseException("Option cannot have value: " + name);
                count++;
                optionHandler();
            }
        }

        private class Option : IOptionParser
        {
            private int count;
            private Action<string> optionHandler;

            public Option(Action<string> optionHandler)
            {
                this.optionHandler = optionHandler;
            }

            public void OnOption(string name, string value)
            {
                if (count > 0)
                    throw new CmdLineParseException("Option cannot be set multiple times: " + name);
                count++;
                optionHandler(value);
            }
        }

        private class RepeatableOption : IOptionParser
        {
            private int count;
            private Action<int, string> optionHandler;

            public RepeatableOption(Action<int, string> optionHandler)
            {
                this.optionHandler = optionHandler;
            }

            public void OnOption(string name, string value)
            {
                optionHandler(count, value);
                count++;
            }
        }
    }

    public class CmdLineParseException : Exception
    {
        public CmdLineParseException(string message)
            : base(message)
        {
        }
    }
}
