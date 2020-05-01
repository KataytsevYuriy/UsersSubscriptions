using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Admin.Models.ViewModels;
using UsersSubscriptions.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using UsersSubscriptions.Common;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UsersConstants.admin)]
    public class SubscriptionController : Controller
    {
        private IAdminDataRepository repository;
        public SubscriptionController(IAdminDataRepository repo) => repository = repo;

        public IActionResult Index(string selectedSchoolId, string selectedCourseId, string MonthStr, string searchName, bool showFilter)
        {
            DateTime dateTime = new DateTime();
            if (!string.IsNullOrEmpty(MonthStr))
            {
                DateTime.TryParse(MonthStr, out dateTime);
            }
            IEnumerable<School> schools = repository.GetAllSchools();
            IEnumerable<Subscription> subscriptions = repository
                .GetFilteredSubscriptions(selectedSchoolId, selectedCourseId, dateTime,searchName);
            SubscriptionsViewModel model = new SubscriptionsViewModel
            {
                _Subscriptions = subscriptions,
                Schools = schools,
                SelectedSchoolId = selectedSchoolId,
                SelectedCourseId = selectedCourseId,
                Month = dateTime,
                SearchName=searchName,
                ShowFilter=showFilter,
            };
            return View(model);
        }

        public async Task<IActionResult> RemoveSubscription(string Id)
        {
            return View(await repository.GetSubscription(Id));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSubscription(Subscription subscription)
        {
            await repository.RemoveSubscriptionAsync(subscription.Id);
            return RedirectToAction(nameof(Index));
        }

    }
}