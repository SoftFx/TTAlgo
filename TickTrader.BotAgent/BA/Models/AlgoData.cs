﻿using NLog;
using System;
using System.IO;
using System.Linq;
using TickTrader.BotAgent.Extensions;

namespace TickTrader.BotAgent.BA.Models
{
    public class AlgoData : IBotFolder
    {
        private static ILogger _log = LogManager.GetLogger(nameof(ClientModel));

        public AlgoData(string path)
        {
            Folder = path;

            EnsureDirectoryCreated();
        }

        public string Folder { get; private set; }

        public IFile[] Files
        {
            get
            {
                if (Directory.Exists(Folder))
                {
                    var dInfo = new DirectoryInfo(Folder);
                    return dInfo.GetFiles().Select(fInfo => new ReadOnlyFileModel(fInfo.FullName)).ToArray();
                }
                else
                    return new ReadOnlyFileModel[0];
            }
        }

        public void Clear()
        {
            if (Directory.Exists(Folder))
            {
                try
                {
                    new DirectoryInfo(Folder).Clean();
                }
                catch (Exception ex)
                {
                    _log.Warn(ex, "Could not clean data folder: " + Folder);
                }

            }
        }

        public void DeleteFile(string file)
        {
            File.Delete(Path.Combine(Folder, file));
        }

        public IFile GetFile(string file)
        {
            if (!file.IsFileNameValid())
                throw new ArgumentException($"Incorrect file name {file}");

            return new ReadOnlyFileModel(Path.Combine(Folder, file));
        }

        public void SaveFile(string file, byte[] bytes)
        {
            if (!file.IsFileNameValid())
                throw new ArgumentException($"Incorrect file name {file}");

            File.WriteAllBytes(Path.Combine(Folder, file), bytes);
        }

        public string GetFileReadPath(string file)
        {
            if (!file.IsFileNameValid())
                throw new ArgumentException($"Incorrect file name {file}");

            return Path.Combine(Folder, file);
        }

        public string GetFileWritePath(string file)
        {
            if (!file.IsFileNameValid())
                throw new ArgumentException($"Incorrect file name {file}");

            EnsureDirectoryCreated();

            return Path.Combine(Folder, file);
        }

        public void EnsureDirectoryCreated()
        {
            if (!Directory.Exists(Folder))
            {
                var dinfo = Directory.CreateDirectory(Folder);
            }
        }
    }
}