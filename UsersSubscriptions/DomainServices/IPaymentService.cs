using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public interface IPaymentService
    {
        IdentityResult UpdateCoursePaymentTypes(string schoolId, string courseId, List<string> pTypes);
        void AddDefaultPaymentTypesToSchool(string schoolId);
        School GetSchoolFinance(string schoolId);
        IdentityResult UpdateSchoolFinance(School school);
        IdentityResult AddSchoolTransaction(SchoolTransaction transaction);
        IEnumerable<PaymentType> GetSchoolPaymentTyapes(string schoolId);
        IdentityResult RemoveLastSchoolTransaction(string schoolId);
    }
}
