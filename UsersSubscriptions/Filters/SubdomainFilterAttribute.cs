using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;

namespace UsersSubscriptions.Filters
{
    public class SubdomainFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            System.Diagnostics.Debug.WriteLine("Hello");
            if (filterContext.HttpContext.Request.GetDisplayUrl() == null)
            {
                return;
            }
            var host = filterContext.HttpContext.Request.Host.Value;
            var index = host.IndexOf(".");
            if (index < 0)
            {
                return;
            }
            var subdomain = host.Substring(0, index);
            string[] blacklist = { "www", "mail" };
            if (blacklist.Contains(subdomain))
            {
                return;
            }
            filterContext.HttpContext.GetRouteData().Values["subdomain"] = subdomain;
        }
    }
}
