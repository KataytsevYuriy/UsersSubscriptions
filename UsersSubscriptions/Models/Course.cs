using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models
{
    public class Course
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<string> TeachersId { get; set; }
    }
}
