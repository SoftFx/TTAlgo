using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TickTrader.Algo.Package;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class SaveFileDialogBeahvior : Behavior<Button>
    {
        private readonly GenericCommand _cmd;

        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register("FileName", typeof(string), typeof(SaveFileDialogBeahvior),
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(SaveFileDialogBeahvior),
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(SaveFileDialogBeahvior),
            new FrameworkPropertyMetadata()
            {
                DefaultValue = PackageHelper.GetPackageExtensions,
            });

        public SaveFileDialogBeahvior()
        {
            _cmd = new GenericCommand(o =>
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    FileName = FileName,
                    InitialDirectory = FilePath,
                    Filter = Filter,
                };

                if (dialog.ShowDialog(Window.GetWindow(this)) == true)
                {
                    FilePath = Path.GetDirectoryName(dialog.FileName);
                    FileName = Path.GetFileName(dialog.FileName);
                }
            });
        }

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Command = _cmd;
        }
    }
}
