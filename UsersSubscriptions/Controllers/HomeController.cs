using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;
using UsersSubscriptions.Services;
using System.IO;
using System.Drawing;
using UsersSubscriptions.DomainServices;
using System.Security.Claims;

namespace UsersSubscriptions.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private IUserService _userService;
        public HomeController(IUserService userService, SignInManager<AppUser> signInManager)
        {
            _userService = userService;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> Index()
        {
            AppUser currentUser = await _userService.GetUserAsync(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (currentUser != null)
            {
                Qrcoder.CreateQrFile(currentUser.Id);
                Byte[] qrFromFile = Qrcoder.GetQrFile(currentUser.Id);
                ViewBag.LoadedQrFile = qrFromFile;
                if (!currentUser.IsActive)
                {
                    ViewBag.ErrorMessage = "Ваш акаунт заблоковано. Будь ласка зв'яжiться з адмiністріцією.";
                }
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
