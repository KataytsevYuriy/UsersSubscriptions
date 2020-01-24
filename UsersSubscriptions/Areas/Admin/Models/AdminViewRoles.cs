using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Areas.Admin.Models
{
    public class AdminViewRoles : IdentityRole
    {
        public IEnumerable<string> names;
    }
}
