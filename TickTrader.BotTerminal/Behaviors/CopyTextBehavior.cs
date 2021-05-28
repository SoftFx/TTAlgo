using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class MenuItemCopyTextBehavior : Behavior<MenuItem>
    {
        private GenericCommand _cmd;


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(MenuItemCopyTextBehavior),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        public MenuItemCopyTextBehavior()
        {
            _cmd = new GenericCommand(o => Clipboard.SetText(Text));
        }


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Command = _cmd;
        }
    }

    internal class ButtonCopyTextBehavior : Behavior<ButtonBase>
    {
        private GenericCommand _cmd;


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(ButtonCopyTextBehavior),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        public ButtonCopyTextBehavior()
        {
            _cmd = new GenericCommand(o => Clipboard.SetText(Text));
        }


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Command = _cmd;
        }
    }
}
