using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Admin.Models;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubscriptionController : Controller
    {
        private IAdminDataRepository repository;
        public SubscriptionController(IAdminDataRepository repo) => repository = repo;
 
        public IActionResult Index()
        {
            return View(repository.GetAllSubscriptions());
        }
    }
}