using NDesk.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.CmdUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            string name = null;
            string path = null;

            var set = new OptionSet()
                .Add("n|name=", p => name = p)
                .Add("p|path=", p => path = p);

            try
            {
                var unparsed = set.Parse(args);
                if (unparsed.Count == 0)
                    throw new Exception("Command is not specified.");
                if (unparsed.Count > 1)
                    throw new Exception("Invalid usage. " + string.Concat(unparsed));

                var command = unparsed[0].ToLower();

                if (command == "package")
                {
                    if (string.IsNullOrEmpty(name))
                        throw new Exception("Package name is not specified!");
                    if (string.IsNullOrEmpty(path))
                        throw new Exception("Path to binaries folder is not specified!");
                    AlgoPackageTool.Package(name, path);
                    Console.WriteLine("Package created.");
                }
                else if (command == "listenv")
                {
                    foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
                        Console.WriteLine(entry.Key + " = " + entry.Value);
                }
                else
                    throw new Exception("Invalid command: " + command);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Environment.ExitCode = -1;
            }
        }
    }
}
