using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Areas.Admin.Models;
using UsersSubscriptions.Common;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =UsersConstants.admin)]
    public class FinanceController : Controller
    {
        private IAdminDataRepository repository;
        private ITeacherRepository repositoryTeacher;
        public FinanceController(IAdminDataRepository repo, ITeacherRepository repoTeacher)
        {
            repository = repo;
            repositoryTeacher = repoTeacher;
        }

        public IActionResult Index()
        {
            return View(repository.GetAllSchools());
        }

        public IActionResult SchoolDetails(string schoolId)
        {
            if (string.IsNullOrEmpty(schoolId)) return BadRequest();
            School school = repository.GetSchooloFinance(schoolId);
            if (school == null) return NotFound();
            return View(school);
        }
    }
}