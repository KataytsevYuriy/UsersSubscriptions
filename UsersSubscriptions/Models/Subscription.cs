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
        public DateTime DayStart { get; set; }
        public DateTime DayFinish { get; set; }

        [StringLength(64)]
        public string CourseId { get; set; }
        public Course Course { get; set; }

        [StringLength(64)]
        public string UserId { get; set; }
        public AppUser AppUser { get; set; }

        public bool WasPayed { get; set; }

        [StringLength(64)]
        public SubscriptionCreatedby CreatedbyTeacher { get; set; }
        public DateTime CreatedDatetime { get; set; }

        [StringLength(64)]
        public SubscriptionPayedTo PyedToTeacher { get; set; }
        public DateTime PayedDtetime { get; set; }
    }

   
}
