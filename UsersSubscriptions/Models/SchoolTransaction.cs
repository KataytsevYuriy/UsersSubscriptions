using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class SchoolTransaction
    {
        [StringLength(64)]
        public string Id { get; set; }
        public int Payed { get; set; }
        public int OldBalance { get; set; }
        public int NewBalance { get; set; }
        public string Description { get; set; }
        public DateTime PayedDateTime { get; set; }

        public string SchoolId { get; set; }
        public School School { get; set; }
    }
}
