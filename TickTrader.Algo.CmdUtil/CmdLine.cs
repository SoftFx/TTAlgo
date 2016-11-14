using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.CmdUtil
{
    internal enum Commands { BuildPackage }

    internal class CmdLine
    {
        public CmdLine(string[] args)
        {
            CmdLineParser parser = new CmdLineParser("");

            parser.OnParameter((count, param) =>
            {
                if (count > 0)
                    throw new CmdLineParseException("Multiple commands are not supported!");

                if (param == "build")
                    Command = Commands.BuildPackage;
                else
                    throw new CmdLineParseException("Unknown command: " + param);
            });

            parser.OnOption("runtime", p => Runtime = p);
            parser.OnOption("ide", p => Ide = p);
            parser.OnOption("workspace", p => Workspace = p);
            parser.OnOption("main", p => MainFile = p);
            parser.OnOption("path", p => FolderPath = p);
            parser.OnOption("output", p => OutputFolder = p);
            parser.OnOption("project", p => ProjectPath = p);
            parser.OnOption("name", p => PckgName = p);

            parser.Parse(args);
        }

        public void PrintUsage()
        {
        }

        public Commands? Command { get; private set; }
        public string Runtime { get; private set; }
        public string Ide { get; private set; }
        public string MainFile { get; private set; }
        public string Workspace { get; private set; }
        public string FolderPath { get; private set; }
        public string ProjectPath { get; private set; }
        public string OutputFolder { get; private set; }
        public string PckgName { get; private set; }
    }
}
