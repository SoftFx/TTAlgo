using System;
using System.Linq;
using System.Text;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    public class FileParamSetupViewModel : ParameterSetupViewModel
    {
        private string _filePath;


        public string DefaultFile { get; private set; }

        public string FileName { get; private set; }

        public string Filter { get; private set; }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                NotifyOfPropertyChange(nameof(FilePath));
                var fileName = "";
                try
                {
                    if (FilePath != null)
                        fileName = System.IO.Path.GetFileName(FilePath);
                }
                catch (ArgumentException) { }
                FileName = fileName;
                NotifyOfPropertyChange(nameof(FileName));

                if (IsRequired && string.IsNullOrWhiteSpace(FileName))
                    Error = new ErrorMsgModel(ErrorMsgCodes.RequiredButNotSet);
                else
                    Error = null;
            }
        }


        public FileParamSetupViewModel(ParameterMetadataInfo descriptor)
            : base(descriptor)
        {
            DefaultFile = descriptor.DefaultValue as string ?? string.Empty;

            var filterEntries = descriptor.FileFilters
               .Where(s => !string.IsNullOrWhiteSpace(s.FileMask) && !string.IsNullOrWhiteSpace(s.FileTypeName));

            var filterStrBuilder = new StringBuilder();
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
            return $"{DisplayName}: {FilePath}";
        }

        public override void Reset()
        {
            FilePath = DefaultFile;
        }

        public override void Load(Property srcProperty)
        {
            var typedSrcProperty = srcProperty as FileParameter;
            if (typedSrcProperty != null)
            {
                FilePath = typedSrcProperty.FileName;
            }
        }

        public override Property Save()
        {
            return new FileParameter() { Id = Id, FileName = FilePath };
        }
    }
}
