﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Models.ViewModels
{
    public class CourseViewModel : Course
    {
        public IList<string> TeachersId { get; set; }
        public IList<string> PTIds { get; set; }
        public IEnumerable<PaymentType> AllPaymentTypes { get; set; }
        public IEnumerable<PaymentType> ListPaymentTypes { get; set; }
        public bool IsItAdmin { get; set; }
        public bool IsCreatingNew { get; set; }
        public CourseViewModel()
        {
            TeachersId = new List<string>();
            PTIds = new List<string>();
            AllPaymentTypes = new List<PaymentType>();
        }
    }
}
