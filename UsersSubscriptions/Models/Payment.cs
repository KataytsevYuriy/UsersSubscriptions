using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class Payment
    {
        [StringLength(64)]
        public string Id { get; set; }
        [StringLength(64)]
        public string SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

        public int Price { get; set; }
        public DateTime DateTime { get; set; }
        [StringLength(64)]
        public string PaymentTypeId { get; set; }
        public PaymentType PaymentType { get; set; }

        [StringLength(64)]
        public string PayedToId { get; set; }
        public AppUser PayedTo { get; set; }
    }
}
