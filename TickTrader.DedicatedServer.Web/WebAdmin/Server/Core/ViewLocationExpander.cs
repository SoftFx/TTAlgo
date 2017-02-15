using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.DedicatedServer.Server.Core
{
    public class ViewLocationExpander: IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            string[] locations = new string[] { "/WebAdmin/Server/Views/{1}/{0}.cshtml", "/WebAdmin/Server/Views/Shared/{0}.cshtml" };
            return locations.Union(viewLocations);
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }
    }
}
