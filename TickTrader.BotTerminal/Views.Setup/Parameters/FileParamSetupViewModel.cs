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
        private readonly bool _isRequired;

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
                NotifyOfPropertyChange(nameof(FullPath));
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
                NotifyOfPropertyChange(nameof(FullPath));
            }
        }

        public string FullPath => string.IsNullOrEmpty(FilePath) ? FileName : Path.Combine(FilePath, FileName);


        public FileParamSetupViewModel(ParameterDescriptor descriptor)
            : base(descriptor)
        {
            _isRequired = descriptor.IsRequired;
            DefaultFile = descriptor.DefaultValue ?? string.Empty;


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
            return $"{DisplayName}: {FullPath}";
        }

        public override void Reset()
        {
            FileName = DefaultFile;
        }

        public override void Load(IPropertyConfig srcProperty)
        {
            if (srcProperty is FileParameterConfig typedSrcProperty)
            {
                FileName = Path.GetFileName(typedSrcProperty.FileName);
                FilePath = Path.GetDirectoryName(typedSrcProperty.FileName);
            }
        }

        public override IPropertyConfig Save()
        {
            return new FileParameterConfig()
            {
                PropertyId = Id,
                FileName = FullPath,
            };
        }


        private void CheckFileName()
        {
            if (_isRequired && string.IsNullOrEmpty(FileName))
            {
                Error = new ErrorMsgModel(ErrorMsgCodes.RequiredButNotSet);
                return;
            }

            var incorrectSymbols = Path.GetInvalidFileNameChars();

            bool hasInvalid = FileName.Any(s => incorrectSymbols.Contains(s));

            Error = hasInvalid ? new ErrorMsgModel(ErrorMsgCodes.InvalidCharacters) : null;
        }
    }
}