using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Controllers
{
    public class StudentController : Controller
    {
        private IUserRepository repository;
        public StudentController(IUserRepository repo) => repository = repo;
 
        public async Task<IActionResult> Index()
        {
            AppUser user = await repository.GetCurentUser(HttpContext);
            return View(user);
        }

        public IActionResult AllCourses()
        {
            return View(repository.GetAllCourses());
        }
    }
}