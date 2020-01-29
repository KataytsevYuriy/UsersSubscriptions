using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IList<AppUser> users;
        public RoleViewModel()
        {
            users = new List<AppUser>();
        }
    }
}
