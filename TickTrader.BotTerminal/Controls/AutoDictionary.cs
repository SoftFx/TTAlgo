using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class AutoDictionary : ResourceDictionary
    {
        private bool isLoaded;
        private string src;

        public new string Source
        {
            get { return src; }
            set
            {
                try
                {
                    src = value;

                    var assembly = Assembly.GetEntryAssembly();
                    var resourcesName = assembly.GetName().Name + ".g";
                    var manager = new ResourceManager(resourcesName, assembly);
                    var resourceSet = manager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

                    var regex = WildcardToRegex(Source);
                    var toMerge = resourceSet
                        .OfType<DictionaryEntry>()
                        .Select(e => GetXamlName((string)e.Key))
                        .Where(n => regex.IsMatch(n));

                    foreach (var src in toMerge)
                        MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(src, UriKind.Relative) });
                }
                catch (Exception)
                {
                }
            }
        }

        private static string GetXamlName(string bamlName)
        {
            if (bamlName.EndsWith(".baml", true, CultureInfo.InvariantCulture))
                return bamlName.Substring(0, bamlName.Length - 5) + ".xaml";
            return bamlName;
        }

        public AutoDictionary()
        {
        }

        protected override void OnGettingValue(object key, ref object value, out bool canCache)
        {
            base.OnGettingValue(key, ref value, out canCache);

            if (!isLoaded)
            {
                isLoaded = true;
            }
        }

        private static Regex WildcardToRegex(string pattern)
        {
            var regexPattern =  "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";
            return new Regex(regexPattern, RegexOptions.IgnoreCase);
        }
    }
}
