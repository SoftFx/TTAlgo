using System.IO;

namespace TickTrader.Algo.Package
{
    public static class PackageHelper
    {
        public const string CsvExtensions = "Table|*.csv";

        public const string TxtExtensions = "Text|*.txt";

        public const string PackageExtensions = "Packages|*.ttalgo";

        public const string PackageAndAllExtensions = "Packages|*.ttalgo|All Files|*.*";


        public static bool IsFileSupported(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".ttalgo" || ext == ".dll";
        }
    }
}
