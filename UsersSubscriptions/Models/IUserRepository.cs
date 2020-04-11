using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public interface IUserRepository
    {
        Task<AppUser> GetCurentUser(HttpContext context);
    }
}
