using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Admin.Models;

namespace UsersSubscriptions.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Course> Courses { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Subscription>()
                .HasOne(s => s.PayedTo).WithMany(ap => ap.SubscriptionPayedTo);
                //.OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Subscription>()
                .HasOne(s => s.ConfirmedBy).WithMany(ap => ap.SubscriptionConfirmedBy)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
