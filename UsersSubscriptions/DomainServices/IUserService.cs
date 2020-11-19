using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public interface IUserService
    {
        Task<AppUser> GetUserAsync(string userId);
        AppUser GetUserByPhone(string phone);
        IEnumerable<AppUser> GetAllUsers();
        IEnumerable<AppUser> FindUserByName(string name);
        Task UpdateUserAsync(AppUser user, IList<string> newUserRoles);
        Task<IdentityResult> DeleteUserAsync(string id);
    }
}
