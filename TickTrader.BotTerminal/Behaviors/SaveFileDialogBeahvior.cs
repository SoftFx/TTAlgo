using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class SaveFileDialogBeahvior : Behavior<Button>
    {
        private GenericCommand cmd;

        public SaveFileDialogBeahvior()
        {
            cmd = new GenericCommand(o =>
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = FilePath;
                var wnd = Window.GetWindow(this);
                dialog.Filter = Filter;
                if (dialog.ShowDialog(wnd) == true)
                    FilePath = dialog.FileName;
            });
        }

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            "FilePath", typeof(string), typeof(SaveFileDialogBeahvior),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(SaveFileDialogBeahvior),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });

        public string FilePath { get { return (string)GetValue(FilePathProperty); } set { SetValue(FilePathProperty, value); } }
        public string Filter { get { return (string)GetValue(FilterProperty); } set { SetValue(FilterProperty, value); } }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Command = cmd;
        }
    }
}
