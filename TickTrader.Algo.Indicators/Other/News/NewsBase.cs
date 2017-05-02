using SoftFx.FxCalendar.Calendar.FxStreet;
using SoftFx.FxCalendar.Models;
using SoftFx.FxCalendar.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Other.News
{
    public abstract class NewsMarkersBase : Indicator
    {
        protected StringBuilder MarkerText = new StringBuilder();
        protected List<FxStreetProvider> NewsProviders = new List<FxStreetProvider>();
        protected List<FxStreetNewsModel> News = new List<FxStreetNewsModel>();

        protected override void Init()
        {
            InitializeNewsProviders();
        }

        public abstract void InitializeNewsProviders();

        protected override void Calculate()
        {
            LoadNews();
            Draw();
        }

        protected virtual void LoadNews()
        {
            News.Clear();

            foreach (var provider in NewsProviders)
                News.AddRange(GetCurrentNews(provider));
        }

        protected abstract void Draw();

        protected virtual Colors ConvertToColor(ImpactLevel impactLvl)
        {
            switch (impactLvl)
            {
                case ImpactLevel.High: return Colors.OrangeRed;
                case ImpactLevel.Medium: return Colors.LightGreen;
                default: return Colors.Gray;
            }
        }

        private IEnumerable<FxStreetNewsModel> GetCurrentNews(FxStreetProvider provider)
        {
            var from = Bars.Count > 1 ? Bars[1].CloseTime : Bars[0].OpenTime;
            var to = Bars[0].CloseTime;

            return provider.GetNews(from, to);
        }
    }
}
