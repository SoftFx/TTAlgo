using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotTerminal
{
    public class OverlayPanel : ContentControl
    {
        public static readonly DependencyProperty OverlayModelProperty =
            DependencyProperty.Register("OverlayModel", typeof(object), typeof(OverlayPanel),
                new FrameworkPropertyMetadata(default(object)));

        public object OverlayModel
        {
            get { return GetValue(OverlayModelProperty); }
            set { SetValue(OverlayModelProperty, value); }
        }
    }
}
