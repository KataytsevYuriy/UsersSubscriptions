using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Models
{
    public class CourseViewModel
    {
        [StringLength(64)]
        public string Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public IList<AppUser> AllTeachers { get; set; }
        public IList<AppUser> Teachers { get; set; }
        public List<string> NewTeachers { get; set; }

        public CourseViewModel()
        {
            AllTeachers = new List<AppUser>();
            Teachers = new List<AppUser>();
            NewTeachers = new List<string>();
        }
    }
}
