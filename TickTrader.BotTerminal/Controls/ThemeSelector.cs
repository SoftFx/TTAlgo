using System;
using System.Collections;
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
        private Theme _selectedTheme;
        private SettingsStorageModel _settingsStorage;


        public static ThemeSelector Instance => (ThemeSelector)App.Current.Resources.MergedDictionaries.FirstOrDefault(d => d is ThemeSelector);


        public IList Themes { get; set; }
        public IEnumerable<string> ThemeNames => Themes.OfType<Theme>().Select(t => t.ThemeName);


        public string SelectedTheme
        {
            get { return _selectedTheme?.ThemeName; }
            set
            {
                if (_selectedTheme == null || _selectedTheme.ThemeName != value)
                {
                    Theme toApply = Themes.OfType<Theme>().FirstOrDefault(t => t.ThemeName == value);
                    if (toApply == null)
                        throw new ArgumentException("Theme not found: " + value);
                    Activate(toApply);
                    _settingsStorage.Theme = SelectedTheme;
                    _settingsStorage.Save();
                }
            }
        }


        public ThemeSelector()
        {
            var collection = new ObservableCollection<Theme>();
            collection.CollectionChanged += (s, a) => EnsureDefaultTheme();
            Themes = collection;
        }


        internal void InitializeSettings(PersistModel storage)
        {
            _settingsStorage = storage.SettingsStorage;
            if (ThemeNames.Contains(_settingsStorage.Theme))
            {
                SelectedTheme = _settingsStorage.Theme;
            }
            else
            {
                _settingsStorage.Theme = SelectedTheme;
                _settingsStorage.Save();
            }
        }


        private void EnsureDefaultTheme()
        {
            if (_selectedTheme == null && Themes.Count > 0)
                Activate((Theme)Themes[0]);
        }


        private void Activate(Theme theme)
        {
            int toRemove = MergedDictionaries.Count;

            // add new resources
            theme.Dictionaries.ForEach(MergedDictionaries.Add);

            // switch styles
            var locator = AppBootstrapper.AutoViewLocator;
            if (locator.StylePostfix != theme.StylePrefix)
            {
                locator.StylePostfix = theme.StylePrefix;
            }

            // remove old resources
            for (int i = 0; i < toRemove; i++) MergedDictionaries.RemoveAt(0);

            // remember selected theme
            _selectedTheme = theme;
        }
    }


    [ContentProperty("Dictionaries")]
    public class Theme
    {
        public string ThemeName { get; set; }

        public string StylePrefix { get; set; }

        public List<ResourceDictionary> Dictionaries { get; set; }


        public Theme()
        {
            Dictionaries = new List<ResourceDictionary>();
        }
    }
}
