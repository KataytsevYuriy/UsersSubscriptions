using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;
using UsersSubscriptions.Services;
using System.IO;
using System.Drawing;

namespace UsersSubscriptions.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private IUserRepository repository;
        public HomeController(IUserRepository repo, SignInManager<AppUser> signInManager)
        {
            repository = repo;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> Index()
        {
            AppUser currentUser = await repository.GetCurentUser(HttpContext);
            if (currentUser != null)
            {
                Qrcoder.CreateQrFile(currentUser.Id);
                Byte[] qrFromFile = Qrcoder.GetQrFile(currentUser.Id);
                ViewBag.LoadedQrFile = qrFromFile;
            }
            else
            {
               await _signInManager.SignOutAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(currentUser);
        }

        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public FileContentResult DownloadFile(string id)
        {
            Byte[] bytes = Qrcoder.GetQrFile(id);
            Response.Headers.Add("Content-Disposition", "attachment; filename=\"abonement.png\"");
            Response.Headers.Add("Content-Type", "application/force-download");
            return File(bytes, "image/png");
        }
    }
}
