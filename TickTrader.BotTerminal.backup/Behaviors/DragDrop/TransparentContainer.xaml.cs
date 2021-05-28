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
using System.Windows.Shapes;

namespace TickTrader.BotTerminal
{
    public partial class TransparentContainer : Window
    {
        public DragSource DragSource
        {
            get { return (DragSource)GetValue(DragSourceProperty); }
            set { SetValue(DragSourceProperty, value); }
        }

        public static readonly DependencyProperty DragSourceProperty =
            DependencyProperty.Register(nameof(DragSource), typeof(DragSource), typeof(TransparentContainer));

        public TransparentContainer(DragSource dragSource)
        {
            InitializeComponent();

            SourceInitialized += TransparentContainerSourceInitialized;
            DragSource = dragSource;
        }

        private void TransparentContainerSourceInitialized(object sender, EventArgs e)
        {
            PresentationSource windowSource = PresentationSource.FromVisual(this);
            IntPtr handle = ((System.Windows.Interop.HwndSource)windowSource).Handle;
            PlatformProxy.MakeTransparent(handle);
        }
    }
}
