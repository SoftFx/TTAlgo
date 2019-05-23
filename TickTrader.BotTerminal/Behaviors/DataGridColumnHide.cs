using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class DataGridColumnHide : Freezable
    {
        public static readonly DependencyProperty IsShownProperty = DependencyProperty.Register("IsShown", typeof(bool),
            typeof(DataGridColumnHide), new PropertyMetadata(false, new PropertyChangedCallback(IsShownPropertyChanged)));

        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility),
            typeof(DataGridColumnHide), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ColumnKeyProperty = DependencyProperty.Register("ColumnKey", typeof(string),
            typeof(DataGridColumnHide), new PropertyMetadata("Unknown"));

        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register("Provider", typeof(ViewModelStorageEntry),
            typeof(DataGridColumnHide), new PropertyMetadata(null, new PropertyChangedCallback(ProviderPropertyChanged)));

        public bool IsShown
        {
            get { return (bool)GetValue(IsShownProperty); }
            set { SetValue(IsShownProperty, value); }
        }

        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            private set { SetValue(VisibilityProperty, value); }
        }

        public string ColumnKey
        {
            get { return (string)GetValue(ColumnKeyProperty); }
            set { SetValue(ColumnKeyProperty, value); }
        }

        internal ViewModelStorageEntry Provider
        {
            get { return (ViewModelStorageEntry)GetValue(ProviderProperty); }
            set { SetValue(ProviderProperty, value); }
        }

        private static void IsShownPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs a)
        {
            var stateObj = (DataGridColumnHide)d;

            if ((bool)a.NewValue)
                stateObj.Visibility = Visibility.Visible;
            else
                stateObj.Visibility = Visibility.Collapsed;

            stateObj.Provider.ChangeProperty(GetKey(stateObj), stateObj.IsShown.ToString());
        }

        private static void ProviderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs a)
        {
            var stateObj = (DataGridColumnHide)d;

            var prop = stateObj.Provider?.GetProperty(GetKey(stateObj));

            if (bool.TryParse(prop?.State, out var show))
                stateObj.IsShown = show;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new DataGridColumnHide();
        }

        private static string GetKey(DataGridColumnHide stateObj)
        {
            return $"Column_{stateObj.ColumnKey}";
        }
    }
}
