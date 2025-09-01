using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MRCMS.Core.Infrastructure
{
    public class ModularViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var originalLocations = viewLocations.ToList();
            var expandedLocations = new List<string>();

            // Add original locations first
            expandedLocations.AddRange(originalLocations);

            // Get the area name from the context
            var areaName = context.Values.TryGetValue("area", out var area) ? area : null;
            var controllerName = context.Values.TryGetValue("controller", out var controller) ? controller : null;

            if (!string.IsNullOrEmpty(areaName))
            {
                // Add module-specific view locations for areas
                // Area name matches module name (e.g., Blog area -> BlogModule folder)
                expandedLocations.AddRange(new[]
                {
                    "/Modules/{2}Module/Views/{1}/{0}.cshtml",
                    "/Modules/{2}Module/Views/Shared/{0}.cshtml",
                    "/Modules/*/Views/{1}/{0}.cshtml",
                    "/Modules/*/Views/Shared/{0}.cshtml"
                });
            }
            else if (!string.IsNullOrEmpty(controllerName))
            {
                // Add module-specific view locations for regular controllers
                expandedLocations.AddRange(new[]
                {
                    "/Modules/{1}Module/Views/{1}/{0}.cshtml",
                    "/Modules/{1}Module/Views/Shared/{0}.cshtml",
                    "/Modules/*/Views/{1}/{0}.cshtml",
                    "/Modules/*/Views/Shared/{0}.cshtml"
                });
            }

            return expandedLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // Store the area and controller names in the context for use in ExpandViewLocations
            if (context.ActionContext.RouteData.Values.TryGetValue("area", out var areaValue))
            {
                context.Values["area"] = areaValue?.ToString();
            }
            
            if (context.ActionContext.RouteData.Values.TryGetValue("controller", out var controllerValue))
            {
                context.Values["controller"] = controllerValue?.ToString();
            }

            // Add a cache-busting value to ensure views are found correctly
            context.Values["customviewlocation"] = "modules";
        }
    }
}