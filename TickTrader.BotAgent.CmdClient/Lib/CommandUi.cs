using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.BotAgent.CmdClient
{
    public class CommandUi
    {
        private List<Tuple<string, Action>> _commands = new List<Tuple<string, Action>>();

        public CommandUi()
        {
        }

        public void RegsiterCommand(string cmdName, Action cmdImpl)
        {
            _commands.Add(new Tuple<string, Action>(cmdName, cmdImpl));
        }

        public event Action OnBeforeCommand = delegate { };

        public void Run()
        {
            var exitCmdNo = _commands.Count + 1;

            while (true)
            {
                OnBeforeCommand();
                for (var i = 0; i < _commands.Count; i++)
                    Console.WriteLine(i + 1 + ". " + _commands[i].Item1);
                Console.WriteLine(exitCmdNo + ". exit");
                Console.Write("cmd>");

                var cmdNo = InputCmd(exitCmdNo);

                if (cmdNo == exitCmdNo)
                    break;

                try
                {
                    var cmdDescriptor = _commands[cmdNo - 1];
                    Console.Clear();
                    _commands[cmdNo - 1].Item2();
                    Console.WriteLine();
                    Console.WriteLine("Command '" + cmdDescriptor.Item1 + "' completed.");
                    Console.WriteLine();
                }
                catch (ApplicationException ex)
                {
                    Console.WriteLine("FAILED! " + ex.Message);
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine();
                }
            }
        }

        public static int InputInteger(string paramName)
        {
            Console.Write(paramName + ">");
            var input = Console.ReadLine();
            if (!int.TryParse(input, out var result))
                throw new ApplicationException("Invalid integer!");
            return result;
        }

        public static int? InputNullabelInteger(string paramName)
        {
            Console.Write(paramName + ">");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (!int.TryParse(input, out var result))
                throw new ApplicationException("Invalid integer!");
            return result;
        }

        public static double InputDouble(string paramName)
        {
            Console.Write(paramName + ">");
            var input = Console.ReadLine();
            if (!double.TryParse(input, out var result))
                throw new ApplicationException("Invalid double!");
            return result;
        }

        public static double? InputNullableDouble(string paramName)
        {
            Console.Write(paramName + ">");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (!double.TryParse(input, out var result))
                throw new ApplicationException("Invalid double!");
            return result;
        }

        public static string InputString(string paramName)
        {
            Console.Write(paramName + ">");
            return Console.ReadLine();
        }

        public static DateTime? InputNullableDateTime(string paramName)
        {
            Console.Write(paramName + ">");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (!DateTime.TryParse(input, out var result))
                throw new ApplicationException("Invalid datetime!");
            return result;
        }

        public static string Choose(string paramName, params string[] choices)
        {
            var index = ChooseIndex(paramName, choices);
            return choices[index];
        }

        public static string ChooseNullable(string paramName, params string[] choices)
        {
            var index = ChooseIndexNullable(paramName, choices);
            if (index == null)
                return null;
            return choices[index.Value];
        }

        public static int ChooseIndex(string paramName, params string[] choices)
        {
            for (var i = 0; i < choices.Length; i++)
                Console.WriteLine(i + 1 + ". " + choices[i]);

            Console.Write(paramName + ">");
            var input = Console.ReadLine();
            if (!int.TryParse(input, out var choice))
                throw new ApplicationException("Invalid integer!");
            if (choice <= 0 || choice > choices.Length)
                Console.WriteLine("Invalid choice!");
            return choice - 1;
        }

        public static int? ChooseIndexNullable(string paramName, params string[] choices)
        {
            for (var i = 0; i < choices.Length; i++)
                Console.WriteLine(i + 1 + ". " + choices[i]);

            Console.Write(paramName + ">");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (!int.TryParse(input, out var choice))
                throw new ApplicationException("Invalid integer!");
            if (choice <= 0 || choice > choices.Length)
                Console.WriteLine("Invalid choice!");
            return choice - 1;
        }

        public static T Choose<T>(string paramName, IEnumerable<T> choices, Func<T, string> nameFunc)
        {
            var names = choices.Select(nameFunc).ToArray();
            var array = choices.ToList();
            var index = ChooseIndex(paramName, names);
            return array[index];
        }

        private int InputCmd(int lastCmdNo)
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (!int.TryParse(input, out var cmdNo))
                    Console.WriteLine("Please enter command number!");
                else if (cmdNo <= 0 || cmdNo > lastCmdNo)
                    Console.WriteLine("No such command!");
                else
                    return cmdNo;
            }
        }
    }
}
