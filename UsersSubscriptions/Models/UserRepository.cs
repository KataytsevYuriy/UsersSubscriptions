using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Data;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Models
{

    public class UserRepository : IUserRepository
    {
        private UserManager<AppUser> _userManager;
        public UserRepository(UserManager<AppUser> manager)
        {
            _userManager = manager;
        }

        public async Task<AppUser> GetCurentUser(HttpContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.User);
            return user;
        }
    }
}
