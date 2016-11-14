using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.VS.Package;

namespace TickTrader.Algo.CmdUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CmdLine cfg = new CmdLine(args);

                if (cfg.Command == Commands.BuildPackage)
                {
                    if (string.IsNullOrEmpty(cfg.FolderPath))
                        throw new Exception("Path is not specified! Use 'path' option to specify path.");

                    if(string.IsNullOrEmpty(cfg.MainFile))
                        throw new Exception("Main file is not specified! Use 'main' option to specify main file!");

                    PackageWriter writer = new PackageWriter();
                    writer.SrcFolder = cfg.FolderPath;
                    writer.Ide = cfg.Ide;
                    writer.MainFileName = cfg.MainFile;
                    writer.Runtime = cfg.Runtime;
                    writer.Workspace = cfg.Workspace;
                    writer.ProjectFile = cfg.ProjectPath;

                    string targetFolder = cfg.OutputFolder;
                    string packageFileName = cfg.PckgName;

                    if (string.IsNullOrEmpty(targetFolder))
                        targetFolder = EnvService.AlgoCommonRepositoryFolder;

                    writer.Save(targetFolder, packageFileName);
                }
                else
                    cfg.PrintUsage();
            }
            catch (CmdLineParseException cex)
            {
                Console.WriteLine("Invalid usage. " + cex.Message);
            }
            catch (System.IO.IOException iox)
            {
                Console.WriteLine(iox.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error: " + ex);
            }
        }
    }
}
