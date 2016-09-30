using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.CmdUtil
{
    public static class AlgoPackageTool
    {
        public static void Package(string packageName, string dllFolderPath)
        {
            string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string repFolder = Path.Combine(docFolder, "BotTrader\\AlgoRepository");

            string pckgFileName = packageName + ".ttalgo";
            string pckgPath = Path.Combine(repFolder, pckgFileName);
            string normalizedFolderPath = dllFolderPath.TrimEnd(Path.DirectorySeparatorChar);
            
            Console.WriteLine("Creating algo package...");
            Console.WriteLine("\tPackage name = " + packageName);
            Console.WriteLine("\tSource folder = " + dllFolderPath);
            Console.WriteLine("\tOutput file  = " + pckgPath);

            using (var pckgFs = new FileStream(pckgPath, FileMode.Create))
            {
                ZipArchive archive = new ZipArchive(pckgFs, ZipArchiveMode.Create);
                var files = Directory.GetFiles(normalizedFolderPath);

                foreach (var filePath in files)
                {
                    var entry = archive.CreateEntry(Path.GetFileName(filePath), CompressionLevel.Optimal);
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        using (var entryStream = entry.Open())
                            fileStream.CopyTo(entryStream);
                    }
                }
            }
        }
    }
}
