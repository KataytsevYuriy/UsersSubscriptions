using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class School
    {
        [StringLength(64)]
        public string Id { get; set; }
        public string Name { get; set; }
        [MaxLength(16)]
        [Required]
        public string UrlName { get; set; }

        public AppUser Owner { get; set; }
        [StringLength(64)]
        public string OwnerId { get; set; }
        public bool Enable { get; set; }

        //Payment
        public int Balance { get; set; }
        public int Price { get; set; }
        public bool IsPayed { get; set; }
        public DateTime PayedMonth { get; set; }
        public DateTime AllowTestUntil { get; set; }

        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<PaymentType> PaymentTypes { get; set; }
        public IEnumerable<SchoolTransaction> SchoolTransactions { get; set; }
    }
    public class SchoolConfiguration : IEntityTypeConfiguration<School>
    {
        public void Configure(EntityTypeBuilder<School> builder)
        {
            builder.HasIndex(b => b.UrlName).IsUnique();
        }
    }
}
