using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class PaymentType
    {
        [StringLength(64)]
        public string Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public int Priority { get; set; }

        [StringLength(64)]
        public string SchoolId { get; set; }

        public IEnumerable<CoursePaymentType> CoursePaymentTypes { get; set; }
        public IEnumerable<Subscription> Subscriptions { get; set; }
        public IEnumerable<Payment> Payments { get; set; }
    }
    public class PaymentTypeView
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
