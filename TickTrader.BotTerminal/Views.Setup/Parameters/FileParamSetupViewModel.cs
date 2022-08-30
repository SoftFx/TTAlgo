using System;
using System.IO;
using System.Linq;
using System.Text;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    public class FileParamSetupViewModel : ParameterSetupViewModel
    {
        private string _filePath;
        private string _fileName;

        public string DefaultFile { get; private set; }

        public string Filter { get; private set; }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (FileName == value)
                    return;

                _fileName = value;
                CheckFileName();
                NotifyOfPropertyChange(nameof(FileName));
            }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (FilePath == value)
                    return;

                _filePath = value;
                NotifyOfPropertyChange(nameof(FilePath));
            }
        }


        public FileParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
            DefaultFile = descriptor.DefaultValue as string ?? string.Empty;

            var filterEntries = descriptor.FileFilters
               .Where(s => !string.IsNullOrWhiteSpace(s.FileMask) && !string.IsNullOrWhiteSpace(s.FileTypeName));

            var filterStrBuilder = new StringBuilder(1 << 4);
            foreach (var entry in filterEntries)
            {
                if (filterStrBuilder.Length > 0)
                    filterStrBuilder.Append('|');
                filterStrBuilder.Append(entry.FileTypeName).Append('|').Append(entry.FileMask);
            }
            Filter = filterStrBuilder.ToString();
        }


        public override string ToString()
        {
            return $"{DisplayName}: {GetFullPath()}";
        }

        public override void Reset()
        {
            FileName = DefaultFile;
        }

        public override void Load(IPropertyConfig srcProperty)
        {
            if (srcProperty is FileParameterConfig typedSrcProperty)
                FileName = Path.GetFileName(typedSrcProperty.FileName);
        }

        public override IPropertyConfig Save()
        {
            return new FileParameterConfig()
            {
                PropertyId = Id,
                FileName = GetFullPath(),
            };
        }

        private string GetFullPath()
        {
            return string.IsNullOrEmpty(FilePath) ? FileName : Path.Combine(FilePath, FileName);
        }

        private void CheckFileName()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                Error = new ErrorMsgModel(ErrorMsgCodes.RequiredButNotSet);
                return;
            }

            var incorrectSymbols = System.IO.Path.GetInvalidFileNameChars();

            bool ok = FileName.All(s => !incorrectSymbols.Contains(s));

            if (!ok)
                Error = new ErrorMsgModel(ErrorMsgCodes.InvalidCharacters);
            else
                Error = null;
        }
    }
}