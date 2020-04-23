using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;

namespace UsersSubscriptions.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser,IdentityRole,string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<CourseAppUser> CourseAppUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new SubscriptionConfiguration());
            builder.ApplyConfiguration(new SchoolConfiguration());
            builder.ApplyConfiguration(new AppUserConfiguration());
         }
    }
}
