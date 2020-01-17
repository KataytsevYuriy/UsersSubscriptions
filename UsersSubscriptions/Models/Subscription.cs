using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class Subscription
    {
        public string Id { get; set; }
        public DateTime DayStart { get; set; }
        public DateTime DayFinish { get; set; }
        public string CourseId { get; set; }
        public string UserId { get; set; }
        public bool WasPayed { get; set; }
        public string CreatedByTeacherId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string PayedToTeacherId { get; set; }
        public DateTime PayedDateTime { get; set; }
    }
}
