using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public class PluginCatalogItem
    {
        public PluginCatalogItem(PluginCatalogKey key, AlgoPluginRef pluginRef, string filePath)
        {
            this.Key = key;
            this.Ref = pluginRef;
            this.FilePath = filePath;
        }

        public PluginCatalogKey Key { get; private set; }
        public AlgoPluginRef Ref { get; private set; }
        public string FilePath { get; private set; }
        public PluginDescriptor Descriptor { get { return Ref.Metadata.Descriptor; } }
        public string DisplayName { get { return Descriptor.UiDisplayName; } }
        public string Category { get { return Descriptor.Category; } }

        internal bool CanBeUseForSnapshot<T>(PluginStorageEntry<T> snapshot) where T : PluginStorageEntry<T>, new()
        {
            return Descriptor.Id == snapshot.DescriptorId && IsPathEquivalent(snapshot.PluginFilePath);
        }

        private bool IsPathEquivalent(string snapshotPath)
        {
            if (FilePath == snapshotPath)
                return true;

            if (!snapshotPath.StartsWith(EnvService.Instance.AlgoCommonRepositoryFolder)
                && !FilePath.StartsWith(EnvService.Instance.AlgoCommonRepositoryFolder)
                && snapshotPath.EndsWith(Key.FileName))
                return true; // match cases when package was in local AlgoRepository, but BotTerminal was in different folder

            return false;
        }
    }
}
