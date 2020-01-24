using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Models
{
    public class AdminDetailViewModel 
    {
        public AppUser User { get; set; }
        public List<IdentityRole> AllRoles { get; set; }
        public IList<string> UserRoles { get; set; }
        public AdminDetailViewModel(AppUser appUser,
                                List<IdentityRole>allRoles,
                                IList<string> userRoles)
        {
            User = appUser;
            AllRoles = allRoles;
            UserRoles = userRoles;
        }
    }
}
