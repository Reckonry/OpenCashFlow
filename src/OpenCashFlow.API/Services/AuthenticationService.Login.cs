using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using global::Shared.Core;
using global::Shared.Models;
using global::Shared.Models.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Attempts to authenticate a user with the provided username and password.
        /// </summary>
        /// <param name="username">The username of the user attempting to log in.</param>
        /// <param name="password">The password of the user attempting to log in.</param>
        /// <param name="cancellationToken">A cancellation token for the async operation.</param>
        /// <returns>
        /// A JWT token string if authentication is successful and the user is active; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method retrieves the user using <c>GetUserByUsernameAndPasswordAsync</c>.
        /// If the user is not found or the password is incorrect, <c>null</c> is returned.
        /// If the user is found but is not active (e.g., <c>EmailConfirmed == false</c> or other business rules),
        /// <c>null</c> is also returned. To provide more granular feedback (e.g., account not active),
        /// you should extend this method to check user status and return a result object with error details.
        /// </remarks>
        public async Task<AuthResult> Authenticate(string username, string password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) new AuthResult() { Success = false, ErrorType = AuthErrorType.InvalidCredentials }; ;

            var user = await _authenticationRepository.GetUserByUsernameAndPasswordAsync(username, password, cancellationToken); // Retrieve the user from the repository
            if (user == null) return new AuthResult() { Success = false, ErrorType = AuthErrorType.InvalidCredentials }; // Verify user exists and password is correct

            if (!user.IsApproved && Configuration.RequiredActiveAccountToLogin == true) return new AuthResult() { Success = false, ErrorType = AuthErrorType.NotActive };

            var companyId = await _companyRepository.GetUserTenantIDAsync(user.UserID, cancellationToken);
            if (companyId == null) return new AuthResult() { Success = false, ErrorType = AuthErrorType.InternalError }; // Ensure TenantID is not null

            // Create the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            var claims = new List<Claim>
            {
                new("Username", user.UserName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.UserFirstName),
                new(ClaimTypes.Surname, user.UserLastName ?? string.Empty),
                new("FullName", user.EmployeeSurnameName ?? string.Empty), 
                new("Timezone", "Europe/Rome"), // TODO: add correct timezone
                new("UserID", user.UserID.ToString()),
                new("TenantID", companyId.ToString()!), // Fix: Use ToString() safely with null-forgiving operator
                new("UserAvatar", user.UserAvatar ?? string.Empty), 
            };

            // Add role claims
            if (user.Roles != null && user.Roles.Count > 0)
                foreach (var role in user.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role.AspNetRole!.RoleName));

            // Retrieve custom claims
            var userClaims = await _authenticationRepository.GetUserClaimsAsync(user.UserID, cancellationToken);
            foreach (var userClaim in userClaims)
            {
                if (!string.IsNullOrEmpty(userClaim.ClaimType) && !string.IsNullOrEmpty(userClaim.ClaimValue))
                    claims.Add(new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            }

            var audiences = _configuration.GetSection("JwtSettings:Audience").Get<string[]?>();
            var audience = audiences != null && audiences.Length > 0
                ? audiences[0]
                : _configuration["JwtSettings:Audience"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = audience,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(Configuration.WebSessionDurationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResult() { Success = true, Token = tokenHandler.WriteToken(token), RequiresPasswordChange = user.UserMustChangePassword };
        }

        public async Task<ApiResponse<string?>> AuthenticateFastAsync(HttpContext httpContext, string pin, string FLCookieValue, CancellationToken cancellationToken)
        {
            //#if DEBUG
            ////todo: solo per debug...
            //var protectedValue = "00000000-0000-0000-0000-000000000001.dd19SRFtEfF1CsgGlwRu0L6HRIZ7Vbv7-_FpZkoGHxc";
            //#else
            if (string.IsNullOrEmpty(FLCookieValue))
                return new ApiResponse<string?>(false, "Cookie not found");
            //#endif
            var (companyID, isValidCookie) = CookieSigner.UnprotectCompanyCookie(FLCookieValue);

            // Invalid signature or corrupted cookie -> delete the cookie and exit
            if (!isValidCookie || companyID == null)
            {
                httpContext.Response.Cookies.Delete(Configuration.FLCookieName);
                return new ApiResponse<string?>(false, "Invalid signature");
            }

            var company = await _companyRepository.GetCompanyByIdAsync((Guid)companyID.Value, cancellationToken);
            if (company == null)
            {
                httpContext.Response.Cookies.Delete(Configuration.FLCookieName);
                return new ApiResponse<string?>(false, "Company not found");
            }

            var user = await _authenticationRepository.GetUserByCompanyIDAndPinAsync(companyID.Value, pin, cancellationToken);
            if (user == null) return new ApiResponse<string?>(false, "User not found");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            var claims = new List<Claim>
            {
                new("Username", user.UserName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.UserFirstName),
                new(ClaimTypes.Surname, user.UserLastName ?? string.Empty),
                new("FullName", user.EmployeeSurnameName ?? string.Empty),
                new("Timezone", "Europe/Rome"), // TODO: add correct timezone
                new("UserID", user.UserID.ToString()),
                new("TenantID", company.TenantID.ToString()),
                new("UserAvatar", user.UserAvatar ?? string.Empty),
            };

            // Add the user's real role claims (no hardcode)
            // If navigation is not loaded, read from DB for safety
            try
            {
                var dbRoles = await _context.AspNetUserRole_DS
                    .AsNoTracking()
                    .Include(ur => ur.AspNetRole)
                    .Where(ur => ur.UserID == user.UserID)
                    .ToListAsync(cancellationToken);

                foreach (var ur in dbRoles)
                {
                    var roleName = ur.AspNetRole?.RoleName;
                    if (!string.IsNullOrWhiteSpace(roleName))
                        claims.Add(new Claim(ClaimTypes.Role, roleName));
                }
            }
            catch
            {
                // If loading fails, do not fail fast login
            }

            // Retrieve any custom user claims
            var userClaimsForFast = await _authenticationRepository.GetUserClaimsAsync(user.UserID, cancellationToken);
            foreach (var userClaim in userClaimsForFast)
            {
                if (!string.IsNullOrEmpty(userClaim.ClaimType) && !string.IsNullOrEmpty(userClaim.ClaimValue))
                    claims.Add(new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            }

            var audiences2 = _configuration.GetSection("JwtSettings:Audience").Get<string[]?>();
            var audience2 = audiences2 != null && audiences2.Length > 0
                ? audiences2[0]
                : _configuration["JwtSettings:Audience"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = audience2,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(Configuration.WebSessionDurationMinutes),
                SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return new ApiResponse<string?>(true, "", jwt);
        }

        public async Task<AuthResult> GenerateFastLoginCookieValueAsync(string username, string password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return new AuthResult() { Success = false, ErrorType = AuthErrorType.InvalidCredentials };

            var user = await _authenticationRepository.GetUserByUsernameAndPasswordAsync(username, password, cancellationToken); // Retrieve the user from the repository
            if (user == null) return new AuthResult() { Success = false, ErrorType = AuthErrorType.InvalidCredentials }; // Verify user exists and password is correct

            Guid? TenantID = await _companyRepository.GetUserTenantIDAsync(user.UserID, cancellationToken);
            if (TenantID == null) return new AuthResult() { Success = false, ErrorType = AuthErrorType.InternalError }; // Ensure TenantID is not null

            var companySecret = await _companyRepository.GetCompanySecretByTenantIDAsync(TenantID.Value, cancellationToken);
            if (string.IsNullOrEmpty(companySecret)) return new AuthResult() { Success = false, ErrorType = AuthErrorType.InternalError }; // Ensure company secret is not null or empty

            return new AuthResult() { Success = true, FastLoginToken = CookieSigner.ProtectCompanyCookie(TenantID.Value, companySecret) };
        }

    }
}
