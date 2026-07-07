using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Contracts.Core
{
    public class Configuration
    {
        public static readonly string AuthCookieName = ".CoreAuth";
        public static readonly string SessionCookieName = ".CoreAuth.session";
        public static readonly int WebSessionDurationMinutes = 15;
        public static readonly int WebSessionRememberDurationMinutes = 60 * 24 * 30; // 30 giorni
        /// <summary>
        /// Nome del cookie per il FastLogin, usato per autenticare l'utente senza password
        /// </summary>
        public static readonly string FLCookieName = ".FLCookie";
        public static readonly int FLCookieDurationMinutes = 60*24*31*12; // un anno 

        public static readonly Guid AdministratorRoleID = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid CompanyAdminRoleID = AdministratorRoleID;
        public static readonly Guid EmployeeRoleID = Guid.Parse("00000000-0000-0000-0000-000000000002");
        public static readonly Guid InstanceAdminRoleID = Guid.Parse("00000000-0000-0000-0000-000000000003");

        public static readonly string CompanyAdminRoleName = "CompanyAdmin";
        public static readonly string EmployeeRoleName = "Employee";
        public static readonly string InstanceAdminRoleName = "InstanceAdmin";

        public static readonly bool RequiredActiveAccountToLogin = true;
    }
}
