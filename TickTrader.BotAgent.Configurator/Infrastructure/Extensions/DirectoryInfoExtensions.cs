using System.IO;

namespace TickTrader.BotAgent.Configurator
{
    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo Clear(this DirectoryInfo directory)
        {
            directory.CheckAndDelete();
            directory.Create();

            return directory;
        }

        public static void CheckAndDelete(this DirectoryInfo directory)
        {
            directory.Refresh();

            if (directory.Exists)
                directory.Delete(true);
        }

        public static void MergeTo(this DirectoryInfo source, DirectoryInfo target)
        {
            if (!source.Exists)
                return;

            if (!target.Exists)
                target.Create();

            foreach (var fileInfo in source.GetFiles())
                fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), overwrite: true);

            foreach (var subFolderInfo in source.GetDirectories())
                subFolderInfo.MergeTo(target.CreateSubdirectory(subFolderInfo.Name));
        }
    }
}
