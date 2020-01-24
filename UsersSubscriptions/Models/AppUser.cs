using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class AppUser : IdentityUser
    {
        [StringLength(64)]
        public override string Id { get; set; }
        [StringLength(50)]
        public string FullName { get; set; }
        public bool IsActive { get; set; }
    }
}
