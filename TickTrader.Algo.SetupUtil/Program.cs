using System;
using System.IO;

namespace TickTrader.Algo.SetupUtil
{
    class Program
    {
        private const string AppDir = "AppDir=";
        private const string OutFile = "OutFile=";


        static void Main(string[] args)
        {
            Console.WriteLine("Working Directory: {0}", Directory.GetCurrentDirectory());

            var scriptBuilder = new UninstallScriptBuilder();

            scriptBuilder.UseAppDir(ReadAppDir(args));

            try
            {
                var script = scriptBuilder.Build();

                var scriptFile = Path.Combine(Directory.GetCurrentDirectory(), $"{ReadOutFile(args)}");
                File.WriteAllText(scriptFile, script);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static string ReadOutFile(string[] args)
        {
            return args.Read(OutFile);
        }

        private static string ReadAppDir(string[] args)
        {
            return args.Read(AppDir);
        }
    }
}
