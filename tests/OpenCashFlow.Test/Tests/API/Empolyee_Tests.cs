using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Contracts.DTOs;
using System.Net;
using System.Text.Json;
using Xunit;

namespace OpenCashFlow.Test.Tests
{
    [Collection("NonParallelCollection")]
    public class EmployeeApiTests 
    {
        private readonly CustomWebApplicationFactory _factory;

        public EmployeeApiTests(CustomWebApplicationFactoryFixture fixture)
        {
            _factory = fixture.Factory;
        }

        #region LOGIN
        //todo: Login [OK] (valid email + password)// ✅ SUCCESS CASES
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should succeed with valid email and password")]
        public void Login_ValidEmailAndPassword_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Login [OK] (uppercase email / leading or trailing spaces normalized)        
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should succeed with normalized email (uppercase, spaces)")]
        public void Login_NormalizedEmail_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Login [FAIL] (non-existent email)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should return JWT token with correct claims")]
        public void Login_ReturnsValidJwtToken_ShouldContainClaims()
        {
            // todo: implement test
        }

        //todo: Login [FAIL] (incorrect password)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should return token with roles, companyID, expiration")]
        public void Login_JwtToken_ShouldIncludeRolesCompanyAndExpiration()
        {
            // todo: implement test
        }

        //todo: Login [FAIL] (disabled / soft-deleted user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should generate refresh token if applicable")]
        public void Login_ShouldGenerateRefreshTokenIfEnabled()
        {
            // todo: implement test
        }

        //todo: Login [FAIL] (user belonging to a disabled or expired company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should audit IP, timestamp, and device info")]
        public void Login_ShouldSaveAuditData()
        {
            // todo: implement test
        }

        //todo: Login [OK] (returns a valid JWT token with correct claims)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should fail with non-existent email")]
        public void Login_NonExistentEmail_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Login [OK] (token includes roles, companyID, and correct expiration)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should fail with incorrect password")]
        public void Login_WrongPassword_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Login [OK] (optional refresh token generated)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should fail if user is disabled or logically deleted")]
        public void Login_DisabledOrDeletedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Login [OK] (auditing: save IP, timestamp, optional device)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should fail if user's company is disabled or contract expired")]
        public void Login_UserCompanyDisabledOrExpired_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Login [FAIL] (too many attempts => lock or rate limit)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/login should fail after too many attempts (rate limit or lockout)")]
        public void Login_TooManyAttempts_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Login [FAIL] (invalid email format)
        [Trait("Layer", "API")]
        [Trait("Feature", "Login")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/auth/login should fail with invalid email format")]
        public void Login_InvalidEmailFormat_ShouldFail()
        {
            // todo: implement test
        }
        
        //todo: Fast Login [OK] (with valid code)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should succeed with valid code")]
        public void FastLogin_ValidCode_ShouldSucceed()
        {
            // todo: implement test
        }
        
        //todo: Fast Login [FAIL] (with invalid code / from another company)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should fail with invalid code or from another company")]
        public void FastLogin_InvalidCodeOrOtherCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Fast Login [FAIL] (with tampered cookie)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should fail with tampered cookie")]
        public void FastLogin_TamperedCookie_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Fast Login [FAIL] (disabled / soft-deleted user)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should fail for disabled or logically deleted user")]
        public void FastLogin_DisabledOrDeletedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Fast Login [FAIL] (user belonging to a disabled or expired company)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should fail if user's company is disabled or contract expired")]
        public void FastLogin_UserCompanyDisabledOrExpired_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Fast Login [OK] (returns a valid JWT token with correct claims)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should return JWT token with correct claims")]
        public void FastLogin_ReturnsValidJwtToken_ShouldContainClaims()
        {
            // todo: implement test
        }

        //todo: Fast Login [OK] (token includes roles, companyID, and correct expiration)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login token should include roles, companyID, expiration")]
        public void FastLogin_JwtToken_ShouldIncludeRolesCompanyAndExpiration()
        {
            // todo: implement test
        }

        //todo: Fast Login [OK] (optional refresh token generated)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should generate refresh token if applicable")]
        public void FastLogin_ShouldGenerateRefreshTokenIfEnabled()
        {
            // todo: implement test
        }

        //todo: Fast Login [OK] (auditing: save IP, timestamp, optional device)        
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should audit IP, timestamp, and device info")]
        public void FastLogin_ShouldSaveAuditData()
        {
            // todo: implement test
        }

        //todo: Fast Login [FAIL] (too many attempts => lock or rate limit)
        [Trait("Layer", "API")]
        [Trait("Feature", "FastLogin")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/fast-login should fail after too many attempts (rate limit or lockout)")]
        public void FastLogin_TooManyAttempts_ShouldFail()
        {
            // todo: implement test
        }
        #endregion

        #region RESET PASSWORD    
        //todo: Reset password [OK] (send email with valid token to registered user)
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should send email with valid token to registered user")]
        public void ResetPassword_ValidEmail_ShouldSendToken()
        {
            // todo: implement test
        }

        //todo: Reset password [FAIL] (non-existent email)
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should fail with non-existent email")]
        public void ResetPassword_NonExistentEmail_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Reset password [FAIL] (correct email but user disabled/deleted/expired company)        
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should fail if user is disabled, deleted, or company expired")]
        public void ResetPassword_DisabledDeletedOrExpiredUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Reset password [FAIL] (invalid email format)
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should fail with invalid email format")]
        public void ResetPassword_InvalidEmailFormat_ShouldFail()
        {
            // todo: implement test
        }


        //todo: Reset password [OK] (valid token allows reset)
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password/confirm should allow reset with valid token")]
        public void ResetPassword_ValidToken_ShouldAllowReset()
        {
            // todo: implement test
        }

        //todo: Reset password [FAIL] (expired or invalid token)
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password/confirm should fail with expired or invalid token")]
        public void ResetPassword_InvalidOrExpiredToken_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Reset password [FAIL] (new password does not meet policy)
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/auth/reset-password/confirm should fail with non-compliant new password")]
        public void ResetPassword_NonCompliantPassword_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Reset password [OK] (once changed, the old password is no longer valid)
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password/confirm should invalidate old password")]
        public void ResetPassword_ShouldInvalidateOldPassword()
        {
            // todo: implement test
        }

        //todo: Reset password [OK] (force logout from all devices) [Figure out how to do this ...]
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password/confirm should force logout from all devices")]
        public void ResetPassword_ShouldForceLogoutFromAllDevices()
        {
            // todo: implement test
        }

        //todo: Reset password [OK] (auditing: reset date, IP, user agent)
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password/confirm should audit reset date, IP, and user agent")]
        public void ResetPassword_ShouldAuditResetData()
        {
            // todo: implement test
        }
        #endregion
        
        #region CREAZIONE        
        //todo: User creation with default role [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/users should create user with default role")]
        public void CreateUser_WithDefaultRole_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Role assignment [OK] (admin assigns "Administrator", "Employee", etc.)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/users/{id}/roles should succeed when admin assigns roles like Administrator or Employee")]
        public void AssignRole_AsAdmin_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Role assignment [FAIL] (unauthorized user attempts assignment)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/users/{id}/roles should fail when unauthorized user attempts assignment")]
        public void AssignRole_UnauthorizedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Role assignment [FAIL] (attempt to assign a role higher than the admin)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/users/{id}/roles should fail when admin tries to assign role higher than their own")]
        public void AssignRole_AdminAssignsHigherRole_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Role assignment [FAIL] (role does not exist)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/users/{id}/roles should fail with non-existent role")]
        public void AssignRole_NonExistentRole_ShouldFail()
        {
            // todo: implement test
        }
        #endregion
        
        // ALTRE
        //todo: Roles list read [OK] (admin)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/roles should return role list for admin user")]
        public void GetRoles_AsAdmin_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Roles list read [FAIL] (basic user)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/roles should fail for basic user")]
        public void GetRoles_AsBasicUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Role removal [OK] (admin updates user role)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/users/{id}/roles should succeed when admin removes user role")]
        public void RemoveRole_AsAdmin_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Role removal [FAIL] (basic user attempts to modify own role)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/users/{id}/roles should fail when basic user tries to modify own role")]
        public void RemoveRole_BasicUserSelfModify_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Role change auditing [OK] (who assigned what to whom, and when)
        [Trait("Layer", "API")]
        [Trait("Feature", "UserRoles")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Role changes should be audited (who assigned what to whom and when)")]
        public void RoleAssignment_ShouldBeAudited()
        {
            // todo: implement test
        }   

    }
}
