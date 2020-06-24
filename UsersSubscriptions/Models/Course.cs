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
    public class Course
    {
        [StringLength(64)]
        public string Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int OneTimePrice { get; set; }
        public bool AllowOneTimePrice { get; set; }

        public IEnumerable<Subscription> Subscriptions { get; set; }
        public IEnumerable<CourseAppUser> CourseAppUsers { get; set; }
        public IEnumerable<CoursePaymentType> CoursePaymentTypes { get; set; }

        public School School { get; set; }
        [StringLength(64)]
        public string SchoolId { get; set; }
    }

    public class CourseAppUser
    {
        [StringLength(64)]
        public string Id { get; set; }

        [StringLength(64)]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [StringLength(64)]
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }

    public class CoursePaymentType
    {
        [StringLength(64)]
        public string Id { get; set; }

        [StringLength(64)]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [StringLength(64)]
        public string PaymentTypeId { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}
