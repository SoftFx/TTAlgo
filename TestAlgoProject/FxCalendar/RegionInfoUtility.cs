using System.Collections.Generic;
using System.Globalization;

namespace TestAlgoProject.FxCalendar
{
    public class RegionInfoUtility
    {
        public static Dictionary<string, HashSet<string>> CurrencyToCountries { get; private set; }

        static RegionInfoUtility()
        {
            CurrencyToCountries = GetCurrenciesToCountries();
        }

        private static Dictionary<string, HashSet<string>> GetCurrenciesToCountries()
        {
            Dictionary<string, HashSet<string>> currencyToCountries = new Dictionary<string, HashSet<string>>();
            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo country = new RegionInfo(culture.LCID);

                if (country.Name == "ME") //FxStreet doesn't support Montenegro
                    continue;

                if (currencyToCountries.ContainsKey(country.ISOCurrencySymbol))
                    currencyToCountries[country.ISOCurrencySymbol].Add(country.Name);
                else
                    currencyToCountries.Add(country.ISOCurrencySymbol, new HashSet<string> { country.Name });
            }

            currencyToCountries["EUR"].Add("EMU"); //European Monetary Union

            return currencyToCountries;
        }
    }
}
