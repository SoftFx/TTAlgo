using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotTerminal
{
    public class ShadowPortal : ContentControl
    {
        public static DependencyProperty IsTopShadowVisibleProperty = DependencyProperty.Register("IsTopShadowVisible", typeof(bool), typeof(ShadowPortal));
        public static DependencyProperty IsBottomShadowVisibleProperty = DependencyProperty.Register("IsBottomShadowVisible", typeof(bool), typeof(ShadowPortal));

        public bool IsTopShadowVisible
        {
            get { return (bool)GetValue(IsTopShadowVisibleProperty); }
            set { SetValue(IsTopShadowVisibleProperty, value); }
        }

        public bool IsBottomShadowVisible
        {
            get { return (bool)GetValue(IsBottomShadowVisibleProperty); }
            set { SetValue(IsBottomShadowVisibleProperty, value); }
        }
    }
}
