using FUNewsSystem.Domain.Consts;
using FUNewsSystem.Domain.Enums.SystemAccount;
using FUNewsSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsSystem.Domain.Extensions.SystemAccounts
{
    public static class SystemAccountExtensions
    {
        public static string GetRoleName(this SystemAccount account)
        {
            if (account.AccountRole == null)
                return "Unknown";

            return Enum.IsDefined(typeof(UserRole), account.AccountRole.Value)
                ? Enum.GetName(typeof(UserRole), account.AccountRole.Value)!
                : "Unknown";
        }

        public static string GetRoleConst(this SystemAccount account)
        {
            return account.AccountRole switch
            {
                (int)UserRole.Staff => CustomRoles.Staff,
                (int)UserRole.Lecturer => CustomRoles.Lecturer,
                (int)UserRole.Admin => CustomRoles.Admin,
                _ => "Unknown"
            };
        }

        public static bool IsAdmin(this SystemAccount account)
        {
            return account.AccountRole == (int)UserRole.Admin;
        }

        public static bool IsLecturer(this SystemAccount account)
        {
            return account.AccountRole == (int)UserRole.Lecturer;
        }

        public static bool IsStaff(this SystemAccount account)
        {
            return account.AccountRole == (int)UserRole.Staff;
        }
    }
}
