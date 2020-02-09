using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public IEnumerable<CourseAppUser> CourseAppUsers { get; set; }
        public IEnumerable<Subscription> Subscriptions { get; set; }
        public IEnumerable<SubscriptionCreatedby> SubscriptionCreatedby { get; set; }
        public IEnumerable<SubscriptionPayedTo> SubscriptionPayedTo { get; set; }
    }
}
