using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class AppIdentityDbContext : IdentityDbContext<User> 
    {
        public AppIdentityDbContext (DbContextOptions<AppIdentityDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Course> Courses { get; set; }
    }
}
