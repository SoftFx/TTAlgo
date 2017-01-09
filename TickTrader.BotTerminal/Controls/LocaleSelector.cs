using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TickTrader.BotTerminal
{
    public class LocaleSelector
    {
        private Locale _selectedLocale;
        private static LocaleSelector instance = new LocaleSelector();

        public ObservableCollection<Locale> Locales { get; set; }

        public static LocaleSelector Instance { get { return instance; } }

        public Locale SelectedLocale
        {
            get
            {
                if (_selectedLocale == null)
                    return null;
                return _selectedLocale;
            }
            set
            {
                if (_selectedLocale == null || _selectedLocale != value)
                {
                    Locale toApply = Locales.FirstOrDefault(l => l == value);
                    if (toApply == null)
                        throw new ArgumentException("Locale not found: " + value);
                    Activate(toApply);
                }
            }
        }

        private LocaleSelector()
        {
            Locales = new ObservableCollection<Locale> {
                new Locale() { Code="en-gb", Name="English" },
                new Locale() { Code="ru-ru", Name="Русский" }
            };

            Activate(Locales.First());
        }

        public void ActivateDefault()
        {
            Activate(Locales.First());
        }

        private void Activate(Locale locale)
        {
            _selectedLocale = locale;
            ResxCore.SetLocale(locale.Code);
        }
    }

    public class Locale
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
