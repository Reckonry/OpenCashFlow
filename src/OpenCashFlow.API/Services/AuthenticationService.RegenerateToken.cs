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
        /// Regenerates a JWT token with updated user claims from the database.
        /// Used after profile updates to refresh the token without requiring logout.
        /// </summary>
        /// <param name="userId">The ID of the user whose token should be regenerated.</param>
        /// <param name="cancellationToken">A cancellation token for the async operation.</param>
        /// <returns>An AuthResult containing the new token if successful.</returns>
        public async Task<AuthResult> RegenerateTokenWithUpdatedClaimsAsync(Guid userId, CancellationToken cancellationToken)
        {
            // Get user from database with updated information
            var user = await _context.AspNetUser_DS
                .AsNoTracking()
                .Include(u => u.Roles)
                    .ThenInclude(r => r.AspNetRole)
                .FirstOrDefaultAsync(u => u.UserID == userId, cancellationToken);

            if (user == null)
                return new AuthResult() { Success = false, ErrorType = AuthErrorType.InternalError };

            var companyId = await _companyRepository.GetUserTenantIDAsync(user.UserID, cancellationToken);
            if (companyId == null)
                return new AuthResult() { Success = false, ErrorType = AuthErrorType.InternalError };

            // Create fresh claims with updated user data
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            var claims = new List<Claim>
            {
                new("Username", user.UserName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.UserFirstName),
                new(ClaimTypes.Surname, user.UserLastName ?? string.Empty),
                new("FullName", user.EmployeeSurnameName ?? string.Empty),
                new("Timezone", "Europe/Rome"),
                new("UserID", user.UserID.ToString()),
                new("TenantID", companyId.ToString()!),
                new("UserAvatar", user.UserAvatar ?? string.Empty),
            };

            // Add role claims
            if (user.Roles != null && user.Roles.Count > 0)
                foreach (var role in user.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role.AspNetRole!.RoleName));

            // Get custom user claims
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

            return new AuthResult() { Success = true, Token = tokenHandler.WriteToken(token), RequiresPasswordChange = false };
        }
    }
}
