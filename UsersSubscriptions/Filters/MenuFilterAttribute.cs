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
using UsersSubscriptions.DomainServices;

namespace UsersSubscriptions.Filters
{
    public class MenuFilterAttribute : ActionFilterAttribute
    {
        private readonly ISchoolService _schoolService;
        private readonly ITeacherService _teacherService;
        private IMemoryCache _cache;
        public MenuFilterAttribute(ISchoolService schoolService, IMemoryCache icache,
            ITeacherService teacherService)
        {
            _schoolService = schoolService;
            _teacherService = teacherService;
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
                string subdomainContext = context.HttpContext.Session.GetString("subdomain");
                if (!string.IsNullOrEmpty(subdomain) && subdomain == subdomainContext)
                {
                    if (context.HttpContext.Session.GetString("showOwnerMenu") != "true")
                    {
                        showOwnerMenu = false;
                    }
                    if (context.HttpContext.Session.GetString("showTeacherMenu") != "true")
                    {
                        showTeacherMenu = false;
                    }

                }
                else
                {
                    school = _schoolService.GetSchoolByUrl(subdomain);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        if (school == null || string.IsNullOrEmpty(school.Id))
                        {
                            showOwnerMenu = false;
                            showTeacherMenu = false;
                        }
                        else
                        {
                            if (context.HttpContext.User.IsInRole(Common.UsersConstants.schoolOwner))
                            {
                                showOwnerMenu = _teacherService.IsItThisSchoolOwner(school.Id, userId);
                            }
                            if (context.HttpContext.User.IsInRole(Common.UsersConstants.teacher))
                            {
                                showTeacherMenu = _teacherService.IsItThisSchoolTeacher(school.Id, userId);
                            }
                        }
                        context.HttpContext.Session.SetString("subdomain", subdomain);
                        context.HttpContext.Session.SetString("showOwnerMenu", showOwnerMenu ? "true" : "false");
                        context.HttpContext.Session.SetString("showTeacherMenu", showTeacherMenu ? "true" : "false");
                    }
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
            base.OnActionExecuted(context);
        }
    }
}
