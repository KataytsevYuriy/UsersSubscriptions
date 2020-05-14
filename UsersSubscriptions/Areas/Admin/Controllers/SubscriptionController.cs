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
        private ITeacherRepository teacherRepository;
        public SubscriptionController(IAdminDataRepository repo, ITeacherRepository teacherRepo)
        {
            repository = repo;
            teacherRepository = teacherRepo;
        }

        public IActionResult Index(string selectedSchoolId, string selectedCourseId, string MonthStr, string searchName, bool showFilter)
        {
            DateTime dateTime = new DateTime();
            if (!string.IsNullOrEmpty(MonthStr))
            {
                DateTime.TryParse(MonthStr, out dateTime);
            }
            IEnumerable<School> schools = repository.GetAllSchools();
            IEnumerable<Subscription> subscriptions = repository
                .GetFilteredSubscriptions(selectedSchoolId, selectedCourseId, dateTime, searchName);
            SubscriptionsViewModel model = new SubscriptionsViewModel
            {
                _Subscriptions = subscriptions,
                Schools = schools,
                SelectedSchoolId = selectedSchoolId,
                SelectedCourseId = selectedCourseId,
                Month = dateTime,
                SearchName = searchName,
                ShowFilter = showFilter,
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult RemoveSubscription(SubscriptionsViewModel subscription)
        {
            teacherRepository.RemoveSubscription(subscription.Id);
            DateTime dateTime = new DateTime();
            if (!string.IsNullOrEmpty(subscription.MonthStr))
            {
                DateTime.TryParse(subscription.MonthStr, out dateTime);
            }
            return RedirectToAction(nameof(Index),
                new
                {
                    selectedSchoolId = subscription.SelectedSchoolId,
                    selectedCourseId = subscription.SelectedCourseId,
                    MonthStr = dateTime,
                    searchName = subscription.SearchName,
                    showFilter = subscription.ShowFilter,
                });
        }
    }
}