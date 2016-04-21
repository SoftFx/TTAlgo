using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TickTrader.BotTerminal
{
    public class HoverComboBox : ComboBox
    {
        private RadioButton _dropDownButton;

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(HoverComboBox), new UIPropertyMetadata(false));
        
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(HoverComboBox), new UIPropertyMetadata(""));

        public HoverComboBox()
        {
            SelectionChanged += HoverComboBox_SelectionChanged;  
        }

        private void HoverComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsChecked = true;
        }

        private void DropDownClose()
        {
            base.SetCurrentValue(HoverComboBox.IsDropDownOpenProperty, false);
        }

        public override void OnApplyTemplate()
        {
            if (_dropDownButton != null)
            {
                _dropDownButton.PreviewMouseLeftButtonDown -= Button_PreviewMouseLeftButtonDown;
            }

            base.OnApplyTemplate();

            _dropDownButton = GetTemplateChild("PART_DropDownButton") as RadioButton;
            if (_dropDownButton != null)
            {
                _dropDownButton.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
            }
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsChecked = true;
            DropDownClose();
        }
    }
}
