using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Models
{
    public class UserViewModel 
    {
        //public AppUser User { get; set; }
        [StringLength(64)]
        public string  Id { get; set; }
        [StringLength(50)]
        public string FullName { get; set; }
        public string IsActive { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<IdentityRole> AllRoles { get; set; }
        public IList<string> UserRoles { get; set; }
        public UserViewModel()
        {
            AllRoles = new List<IdentityRole>();
            UserRoles = new List<string>();
        }
    }
}
