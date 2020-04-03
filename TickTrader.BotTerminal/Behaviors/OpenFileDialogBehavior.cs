using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using TickTrader.BotTerminal.Lib;
using Microsoft.Win32;
using System.Windows.Data;
using System.IO;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class OpenFileDialogBehavior : Behavior<Button>
    {
        private readonly GenericCommand _cmd;

        public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register("FileName", typeof(string), typeof(OpenFileDialogBehavior),
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(OpenFileDialogBehavior),
            new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(OpenFileDialogBehavior),
            new FrameworkPropertyMetadata()
            {
                DefaultValue = PackageWatcher.GetPackageExtensions,
            });

        public OpenFileDialogBehavior()
        {
            _cmd = new GenericCommand(o =>
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    FileName = FileName,
                    InitialDirectory = FilePath,
                    Filter = Filter,
                    CheckFileExists = true,
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
