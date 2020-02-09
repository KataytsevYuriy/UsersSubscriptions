using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class SubscriptionCreatedby
    {
        [StringLength(64)]
        public string Id { get; set; }

        [StringLength(64)]
        public string SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

        [StringLength(64)]
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
