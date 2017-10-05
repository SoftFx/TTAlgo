using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TickTrader.BotTerminal
{
    public enum PathInputModes { SaveFile, OpenFile, PickFolder }

    public partial class PathBox : UserControl
    {
        public static readonly DependencyProperty PathModeProperty =
           DependencyProperty.Register(nameof(Mode), typeof(PathInputModes), typeof(PathBox));

        public static DependencyProperty FileFilterProperty =
           DependencyProperty.Register(nameof(Filter), typeof(string), typeof(PathBox));

        public static DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(string), typeof(PathBox));

        public PathInputModes Mode
        {
            get { return (PathInputModes)GetValue(PathModeProperty); }
            set { SetValue(PathModeProperty, value); }
        }

        public string Filter
        {
            get { return (string)GetValue(FileFilterProperty); }
            set { SetValue(FileFilterProperty, value); }
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public PathBox()
        {
            InitializeComponent();
        }

        private void Explore_Click(object sender, RoutedEventArgs e)
        {
            if (Mode == PathInputModes.SaveFile)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.FileName = Path;
                var wnd = Window.GetWindow(this);
                dialog.Filter = Filter;
                if (dialog.ShowDialog(wnd) == true)
                    Path = dialog.FileName;
            }
            else if (Mode == PathInputModes.OpenFile)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.FileName = Path;
                var wnd = Window.GetWindow(this);
                dialog.Filter = Filter;
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog(wnd) == true)
                    Path = dialog.FileName;
            }
            else
                throw new Exception("Mode " + Mode + " is not supported!");
        }
    }
}
