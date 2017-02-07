using System.Globalization;
using System.Resources;
using TickTrader.Algo.GuiModel;

namespace TickTrader.BotTerminal
{
    public class ResxCore :  ObservableObject
    {
        private readonly static ResourceManager _resxmanager;
        private static ResxCore _localeCore;
        private CultureInfo _currentCulture;

        public static ResxCore Instance { get { return _localeCore ?? (_localeCore = new ResxCore()); } }

        public object this[string key, CultureInfo culture = null]
        {
            get
            {
                return _resxmanager.GetObject(key, culture ?? CurrentCulture) ?? string.Format("$${0}$$", key);
            }
        }

        public CultureInfo CurrentCulture
        {
            get
            {
                //TODO: change on Thread.CurrentThread.CurrentUICulture ?
                return _currentCulture;
            }
            private set
            {
                _currentCulture = value;
                NotifyOfPropertyChange("CurrentCulture");
            }
        }

        static ResxCore()
        {
            _resxmanager = new ResourceManager("TickTrader.BotTerminal.Resx.Locales.gui", typeof(ResxCore).Assembly);
        }

        public ResxCore()
        {
            CurrentCulture = CultureInfo.CurrentCulture;
        }

        public static void SetLocale(string localeCode)
        {
            Instance.CurrentCulture = CultureInfo.GetCultureInfo(localeCode);
        }

    }
}
