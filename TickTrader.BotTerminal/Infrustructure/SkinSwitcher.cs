using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class SkinSwitcher : PropertyChangedBase
    {
        private string selected;
        private Dictionary<string, SkinInfo> skins = new Dictionary<string, SkinInfo>();
        private AutoViewManager viewManager;
        private ThemeSelector resxSelector;

        public SkinSwitcher(AutoViewManager viewManager, ThemeSelector resxSelector)
        {
            this.viewManager = viewManager;
            this.resxSelector = resxSelector;
        }

        public void Add(string skinName, string stylePostfix, string resxThemeName)
        {
            skins.Add(skinName, new SkinInfo() { Name = skinName, Postfix = stylePostfix, ResxName = resxThemeName });
        }

        public IEnumerable<string> Skins { get { return skins.Keys; } }

        public string SelectedSkin
        {
            get { return selected; }
            set
            {
                var skinInfo = skins[value];

                if (resxSelector.SelectedTheme != skinInfo.ResxName)
                    resxSelector.SelectedTheme = skinInfo.ResxName;

                if (viewManager.StylePostfix != skinInfo.Postfix)
                    viewManager.StylePostfix = skinInfo.Postfix;

                selected = value;
                NotifyOfPropertyChange(nameof(SelectedSkin));
            }
        }

        public class SkinInfo
        {
            public string Name { get; set; }
            public string Postfix { get; set; }
            public string ResxName { get; set; }
        }
    }
}
