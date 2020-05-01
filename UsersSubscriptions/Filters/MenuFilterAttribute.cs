using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UsersSubscriptions.Data;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Filters
{
    public class MenuFilterAttribute : ActionFilterAttribute
    {
        private readonly ITeacherRepository repository;
        private IMemoryCache _cache;
        public MenuFilterAttribute(ITeacherRepository repo, IMemoryCache icache)
        {
            repository = repo;
            _cache = icache;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            string userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            string subdomain = (context.HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString();
            School school = null;
            bool showTeacherMenu = true;
            bool showOwnerMenu = true;
            if (!string.IsNullOrEmpty(subdomain))
            {
                if (_cache.TryGetValue(subdomain+userId, out CacheMenuModel cacheMenu))
                {
                    school = new School
                    {
                        Name = cacheMenu.Name,
                    };
                    showOwnerMenu = cacheMenu.ShowOwnerMenu;
                    showTeacherMenu = cacheMenu.ShowTeacherMenu;
                }
                else
                {
                    school = repository.GetSchoolByUrl(subdomain);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        if (context.HttpContext.User.IsInRole(Common.UsersConstants.schoolOwner))
                        {
                            showOwnerMenu = repository.IsItThisSchoolOwner(school.Id, userId);
                        }
                        if (context.HttpContext.User.IsInRole(Common.UsersConstants.teacher))
                        {
                            showTeacherMenu = repository.IsItThisSchoolTeacher(school.Id, userId);
                        }
                    }
                    cacheMenu = new CacheMenuModel
                    {
                        Name = school.Name,
                        ShowTeacherMenu = showTeacherMenu,
                        ShowOwnerMenu = showOwnerMenu,
                    };
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3));
                    _cache.Set(subdomain + userId, cacheMenu, cacheEntryOptions);
                }

            }
            Controller controller = context.Controller as Controller;
            if (controller != null)
            {
                if (school != null)
                {
                    controller.ViewData["schoolName"] = school.Name;
                }
                controller.ViewData["showTeacherMenu"] = showTeacherMenu;
                controller.ViewData["showOwnerMenu"] = showOwnerMenu;
            }
        }
    }
}
