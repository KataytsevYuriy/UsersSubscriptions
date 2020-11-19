using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;

namespace UsersSubscriptions.DomainServices
{
    public interface ISchoolService
    {
        School GetSchool(string schoolId);
        IdentityResult UpdateSchoolOptions(School school);
        SchoolCalculationsViewModel GetSchoolDetail(string schoolId, string courseId, DateTime month, string selectedNavId, string selectedTeacherId);
        School GetSchoolByUrl(string url);
        IEnumerable<School> GetUsersSchools(string userId);
        IEnumerable<Subscription> GetTeacherMonthSubscriptions(string courseId, DateTime month);
        //IEnumerable<PaymentType> GetSchoolPaymentTyapes(string schoolId);
        bool IsSchoolAllowed(string schoolId);
        IdentityResult PayForSchool(string schoolId, string description);
        IEnumerable<School> GetAllSchools();
        Task<IdentityResult> CreateSchoolAsync(School school);
        Task<IdentityResult> UpdateSchoolAsync(School school);
        Task<IdentityResult> RemoveScoolAsync(string Id);
        Task<IdentityResult> ChengeOwnerAsync(string newOwnerId, string schoolId);
    }
}
