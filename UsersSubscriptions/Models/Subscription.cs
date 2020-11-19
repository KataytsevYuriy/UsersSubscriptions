using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class Subscription
    {
        [StringLength(64)]
        public string Id { get; set; }
        [DisplayFormat(DataFormatString ="{0:dd'/'mm'/'yyyy}", ApplyFormatInEditMode =true)]
        //public DateTime Month { get; set; }

        //public string Comment { get; set; }

        public DateTime Period { get; set; }
        [StringLength(64)]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [StringLength(64)]
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        [StringLength(50)]
        public string FullName { get; set; }
        [Phone]
        public string Phone { get; set; }
        public bool MonthSubscription { get; set; }

        public int Price { get; set; }

        public DateTime CreatedDatetime { get; set; }

        //[StringLength(64)]
        //public string PayedToId { get; set; }
        //public AppUser PayedTo { get; set; }
        //public DateTime PayedDatetime { get; set; }
        public IEnumerable<Payment> Payments { get; set; }
        //[StringLength(64)]
        //public string PaymentTypeId { get; set; }
        //public PaymentType PaymentType { get; set; }
    }

    //public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    //{
    //    public void Configure(EntityTypeBuilder<Subscription> builder)
    //    {
    //        //builder.HasOne(s => s.PayedTo).WithMany(s => s.Subscriptions);
    //    }
    //}


}
