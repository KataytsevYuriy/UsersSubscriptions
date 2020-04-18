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

        public AppUser Owner { get; set; }
        [StringLength(64)]
        public string OwnerId { get; set; }

        public IEnumerable<Course> Courses { get; set; }
    }
}
