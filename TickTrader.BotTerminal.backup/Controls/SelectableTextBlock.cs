using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class SelectableTextBlock : TextBlock
    {
        public string FullText
        {
            get { return (string)GetValue(FullTextProperty); }
            set { SetValue(FullTextProperty, value); }
        }

        public static readonly DependencyProperty FullTextProperty = DependencyProperty.Register("FullText", typeof(string), typeof(SelectableTextBlock),
                new UIPropertyMetadata(string.Empty, UpdateControlCallBack));

        public static readonly DependencyProperty ToSearchProperty = DependencyProperty.Register("ToSearch", typeof(string), typeof(SelectableTextBlock),
         new UIPropertyMetadata(string.Empty, UpdateControlCallBack));

        public string ToSearch
        {
            get { return (string)GetValue(ToSearchProperty); }
            set { SetValue(ToSearchProperty, value); }
        }

        /// <summary>
        /// Create a call back function which is used to invalidate the rendering of the element, 
        /// and force a complete new layout pass.
        /// One such advanced scenario is if you are creating a PropertyChangedCallback for a 
        /// dependency property that is not  on a Freezable or FrameworkElement derived class that 
        /// still influences the layout when it changes.
        /// </summary>
        private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (SelectableTextBlock)d;
            obj.InvalidateVisual();
            obj.UpdateInlines();
        }

        static readonly SolidColorBrush BackgroundBrush = new SolidColorBrush(Colors.Yellow);

        private void UpdateInlines()
        {
            Inlines.Clear();
            var fullText = FullText;
            var toSearch = ToSearch;

            if (string.IsNullOrEmpty(toSearch))
            {
                Inlines.Add(fullText);
                return;
            }

            do
            {
                var index = fullText.IndexOf(toSearch, StringComparison.CurrentCultureIgnoreCase);

                if (index < 0)
                {
                    Inlines.Add(new Run(fullText));
                    break;
                }

                Inlines.AddRange(new Inline[]
                {
                        new Run(fullText.Substring(0, index)),
                        new Run(fullText.Substring(index, toSearch.Length))
                        {
                            Background = BackgroundBrush,
                            //Foreground = forecolor
                        }
                });

                fullText = fullText.Substring(index + toSearch.Length);
            }
            while (true);
        }
    }
}
