using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace TickTrader.BotTerminal
{
    [TemplatePart(Name = "PART_SplitButton", Type = typeof(SplitButton))]
    public class DropDownMenuButton : ContentControl
    {
        private SplitButton _button;

        public DropDownMenuButton()
        {
            AddHandler(MenuItem.ClickEvent, new RoutedEventHandler((s, e) =>
            {
                e.Handled = true;
                if (_button != null)
                    _button.IsOpen = false;
                ClickedItem = ((MenuItem)e.OriginalSource).DataContext;
                RaiseEvent(new RoutedEventArgs(ItemClickEvent, this));
            }));
        }

        public static RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(DropDownMenuButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public static RoutedEvent ItemClickEvent = EventManager.RegisterRoutedEvent("ItemClick", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(DropDownMenuButton));

        public event RoutedEventHandler ItemClick
        {
            add { AddHandler(ItemClickEvent, value); }
            remove { RemoveHandler(ItemClickEvent, value); }
        }

        public static DependencyProperty ClickedItemProperty = DependencyProperty.Register("ClickedItem",
            typeof(object), typeof(DropDownMenuButton));

        public object ClickedItem
        {
            get { return GetValue(ClickedItemProperty); }
            set { SetValue(ClickedItemProperty, value); }
        }

        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(IEnumerable), typeof(DropDownMenuButton));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_button != null)
                _button.Click -= _button_Click;

            _button = GetTemplateChild("PART_SplitButton") as SplitButton;

            if (_button != null)
                _button.Click += _button_Click;
        }

        private void _button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            e.Handled = true;
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
    }
}
