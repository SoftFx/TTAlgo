using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace TickTrader.BotTerminal
{
    [ContentProperty("Themes")]
    public class ThemeSelector : ResourceDictionary
    {
        public static ThemeSelector Instance
        {
            get { return (ThemeSelector)App.Current.Resources.MergedDictionaries.FirstOrDefault(d => d is ThemeSelector); }
        }

        private Theme selectedTheme;

        public ThemeSelector()
        {
            Themes = new ObservableCollection<Theme>();
            Themes.CollectionChanged += (s, a) => EnsureDefaultTheme();
        }

        private void EnsureDefaultTheme()
        {
            if (selectedTheme == null && Themes.Count > 0)
                Activate(Themes[0]);
        }

        private void Activate(Theme theme)
        {
            int toRemove = MergedDictionaries.Count;
            theme.Dictionaries.ForEach(MergedDictionaries.Add);
            for (int i = 0; i < toRemove; i++) MergedDictionaries.RemoveAt(0);

            selectedTheme = theme;
        }

        public ObservableCollection<Theme> Themes { get; set; }
        public IEnumerable<string> ThemeNames { get { return Themes.Select(t => t.Name); } }

        public string SelectedTheme
        {
            get
            {
                if (selectedTheme == null)
                    return null;
                return selectedTheme.Name;
            }
            set
            {
                if (selectedTheme == null || selectedTheme.Name != value)
                {
                    Theme toApply = Themes.FirstOrDefault(t => t.Name == value);
                    if (toApply == null)
                        throw new ArgumentException("Theme not found: " + value);
                    Activate(toApply);
                }
            }
        }
    }

    [ContentProperty("Dictionaries")]
    public class Theme
    {
        public Theme()
        {
            Dictionaries = new List<ResourceDictionary>();
        }

        public string Name { get; set; }
        public List<ResourceDictionary> Dictionaries { get; set; }
    }
}
