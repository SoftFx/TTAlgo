using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class PathInputViewModel : ObservableObject
    {
        private string _path;

        public PathInputViewModel(PathInputModes mode)
        {
            Mode = mode;
        }

        public bool IsValid { get; private set; }
        public string ValidationError { get; set; }
        public string Filter { get; set; }
        public PathInputModes Mode { get; }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                IsValid = ValidatePath(value);
                NotifyOfPropertyChange(nameof(Path));
                NotifyOfPropertyChange(nameof(IsValid));
            }
        }

        private bool ValidatePath(string path)
        {
            return !string.IsNullOrWhiteSpace(path);
        }
    }

    internal enum PathInputModes { OpenFile, Folder, SaveFile }
}
