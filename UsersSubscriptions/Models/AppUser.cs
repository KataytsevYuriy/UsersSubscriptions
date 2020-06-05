using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        [Key]
        public override string Id { get; set; }
        [StringLength(50, MinimumLength = 5)]
        public string FullName { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<CourseAppUser> CourseAppUsers { get; set; }
        public IEnumerable<Subscription> Subscriptions { get; set; }
        public IEnumerable<Subscription> SubscriptionPayedTo { get; set; }
        public IEnumerable<School> Schools { get; set; }
    }
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasIndex(b => b.PhoneNumber).IsUnique();
        }
    }
}
