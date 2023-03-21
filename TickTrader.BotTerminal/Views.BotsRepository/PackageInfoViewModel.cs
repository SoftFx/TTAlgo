using Machinarium.Var;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.WpfWindowsSupportLibrary;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    public sealed class PackageInfoViewModel
    {
        private readonly Dictionary<string, PluginModelInfo.Types.PluginState> _botsStates = new();
        private readonly VarContext _context = new();


        public string Id { get; }

        public string Size { get; }

        public string Path { get; }

        public string LastModify { get; }

        public string CreatedDate { get; }


        public BoolProperty CanRemove { get; }

        public StrProperty DisabledRemoveTooltip { get; }


        internal PackageInfoViewModel(string id, PackageIdentity identity)
        {
            Id = id;
            Size = identity?.Size.ToKB();
            Path = identity?.FilePath;
            LastModify = identity?.LastModifiedUtc.ToDefaultTime();
            CreatedDate = identity?.CreatedUtc.ToDefaultTime();

            CanRemove = _context.AddBoolProperty(true);
            DisabledRemoveTooltip = _context.AddStrProperty();
        }


        public void OpenExplorer()
        {
            var path = System.IO.Path.GetDirectoryName(Path);

            try
            {
                Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                MessageBoxManager.OkError(ex.Message);
            }
        }

        public void RemovePackage()
        {
            if (MessageBoxManager.YesNoBoxQuestion("Are you sure you want to remove the selected package?"))
            {
                try
                {
                    File.Delete(Path);
                }
                catch (Exception ex)
                {
                    MessageBoxManager.OkError(ex.Message);
                }
            }
        }

        internal void ChangeBotStatus(string key, PluginModelInfo.Types.PluginState status)
        {
            _botsStates[key] = status;

            var runningCnt = _botsStates.Values.Count(u => u == PluginModelInfo.Types.PluginState.Running);
            
            CanRemove.Value = runningCnt == 0;
            DisabledRemoveTooltip.Value = $"Cannot remove the package. {runningCnt} {(runningCnt > 1 ? "bots are" : "bot is")} running!";
        }
    }
}
