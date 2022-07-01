using Caliburn.Micro;
using System.Globalization;
using System.Resources;

namespace TickTrader.BotTerminal
{
    public class ResxCore : PropertyChangedBase
    {
        private readonly static ResourceManager _resxmanager;
        private static ResxCore _localeCore;

        private CultureInfo _currentCulture;


        public object this[string key, CultureInfo culture = null] => _resxmanager.GetString(key, culture ?? CurrentCulture) ?? $"$${key}$$";

        public static ResxCore Instance { get; } = _localeCore ??= new ResxCore();


        public CultureInfo CurrentCulture
        {
            get => _currentCulture;

            private set
            {
                if (_currentCulture == value)
                    return;

                _currentCulture = value;

                NotifyOfPropertyChange(nameof(CurrentCulture));
            }
        }


        static ResxCore()
        {
            _resxmanager = new ResourceManager("TickTrader.BotTerminal.Resx.Locales.gui", typeof(ResxCore).Assembly)
            {
                IgnoreCase = true,
            };
        }

        public ResxCore()
        {
            CurrentCulture = CultureInfo.InvariantCulture;
        }


        public static void SetLocale(string localeCode)
        {
            Instance.CurrentCulture = CultureInfo.GetCultureInfo(localeCode);
        }
    }
}
