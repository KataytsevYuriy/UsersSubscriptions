using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using UsersSubscriptions.Common;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UsersConstants.admin)]
    public class SubscriptionController : Controller
    {
        private IAdminDataRepository repository;
        public SubscriptionController(IAdminDataRepository repo) => repository = repo;
 
        public IActionResult Index()
        {
            var ttt = repository.GetAllSubscriptions();
            return View(repository.GetAllSubscriptions());
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