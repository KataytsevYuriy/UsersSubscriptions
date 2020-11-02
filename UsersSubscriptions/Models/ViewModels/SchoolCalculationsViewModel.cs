using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models.ViewModels
{
    public class SchoolCalculationsViewModel
    {
        public IEnumerable<SchoolCourse> SchoolCourses { get; set; }
        public Dictionary<string,int> SchoolCoursesByPaymentTypeSum { get; set; }
        public IEnumerable<Subscription> CourseSubscriptions { get; set; }
        public IEnumerable<Subscription> TeacherSubscriptions { get; set; }
        public string SelectedCourseId { get; set; }
        public string SelectedTeacherId { get; set; }
        public IEnumerable<SchoolTeacher> SchoolTeachers { get; set; }
        public string SchoolId { get; set; }
        public DateTime Month { get; set; }
        public string SelectedNavId { get; set; }

    }
    public class SchoolCourse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int Sum { get; set; }
    }
    public class SchoolTeacher
    {
        public string Id { get; set; }
        public string FullName { get; set; }
    }
}
