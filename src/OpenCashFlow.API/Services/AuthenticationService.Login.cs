using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Auth.FastLogin;
using OpenCashFlow.Application.Auth.Login;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Contracts.Audit;
using OpenCashFlow.Contracts.Core;
using OpenCashFlow.Contracts.Security;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService : IAuthenticationService
    {
        private static readonly ConcurrentDictionary<string, FastLoginAttemptState> FastLoginAttempts = new();

        public async Task<AuthResult> Authenticate(string username, string password, CancellationToken cancellationToken)
        {
            var result = await _loginUseCase.ExecuteAsync(
                new LoginCommand(
                    username,
                    password,
                    Configuration.RequiredActiveAccountToLogin,
                    GetSecurityInt("Security:SessionMinutes", Configuration.WebSessionDurationMinutes)),
                cancellationToken);

            if (!result.Success)
            {
                var auditReason = result.ErrorType switch
                {
                    AuthFailure.NotActive => "Account not active",
                    AuthFailure.InternalError => "Company not found",
                    _ => "Invalid credentials"
                };

                await WriteAuthenticationAuditAsync(
                    AuditEventType.LoginFailed,
                    "LoginFailed",
                    username,
                    result.UserID,
                    result.TenantID,
                    auditReason,
                    cancellationToken);

                return new AuthResult { Success = false, ErrorType = MapAuthError(result.ErrorType) };
            }

            await WriteAuthenticationAuditAsync(
                AuditEventType.Login,
                "Login",
                username,
                result.UserID,
                result.TenantID,
                null,
                cancellationToken);

            return new AuthResult
            {
                Success = true,
                Token = result.Token,
                RequiresPasswordChange = result.RequiresPasswordChange
            };
        }

        public async Task<ApiResponse<string?>> AuthenticateFastAsync(HttpContext httpContext, string pin, string FLCookieValue, CancellationToken cancellationToken)
        {
            if (!GetSecurityBool("Security:FastLogin:Enabled", false))
            {
                await WriteAuthenticationAuditAsync(AuditEventType.LoginFailed, "FastLoginDisabled", null, null, null, "Fast login disabled", cancellationToken);
                return new ApiResponse<string?>(false, "Fast login disabled");
            }

            if (string.IsNullOrEmpty(FLCookieValue))
            {
                await WriteAuthenticationAuditAsync(AuditEventType.LoginFailed, "FastLoginFailed", null, null, null, "Cookie not found", cancellationToken);
                return new ApiResponse<string?>(false, "Cookie not found");
            }

            var attemptKey = BuildFastLoginAttemptKey(httpContext, FLCookieValue);
            if (IsFastLoginLocked(attemptKey, out var lockoutSeconds))
            {
                await WriteAuthenticationAuditAsync(AuditEventType.LoginFailed, "FastLoginLocked", null, null, null, $"Fast login locked for {lockoutSeconds} seconds", cancellationToken);
                return new ApiResponse<string?>(false, "Too many attempts. Try again later.");
            }

            var result = await _fastLoginUseCase.ExecuteAsync(
                new FastLoginCommand(
                    pin,
                    FLCookieValue,
                    GetSecurityInt("Security:SessionMinutes", Configuration.WebSessionDurationMinutes)),
                cancellationToken);

            if (!result.Success)
            {
                RegisterFastLoginFailure(attemptKey);

                if (result.Message is "Invalid signature" or "Company not found")
                {
                    httpContext.Response.Cookies.Delete(Configuration.FLCookieName);
                }

                await WriteAuthenticationAuditAsync(
                    AuditEventType.LoginFailed,
                    "FastLoginFailed",
                    result.UserName,
                    result.UserID,
                    result.TenantID,
                    result.Message,
                    cancellationToken);

                return new ApiResponse<string?>(false, result.Message);
            }

            ClearFastLoginFailures(attemptKey);

            await WriteAuthenticationAuditAsync(
                AuditEventType.Login,
                "FastLogin",
                result.UserName,
                result.UserID,
                result.TenantID,
                null,
                cancellationToken);

            return new ApiResponse<string?>(true, "", result.Token);
        }

        public async Task<AuthResult> GenerateFastLoginCookieValueAsync(string username, string password, CancellationToken cancellationToken)
        {
            var result = await _generateFastLoginCookieUseCase.ExecuteAsync(
                new GenerateFastLoginCookieCommand(
                    username,
                    password,
                    GetSecurityBool("Security:FastLogin:Enabled", false)),
                cancellationToken);

            return new AuthResult
            {
                Success = result.Success,
                FastLoginToken = result.FastLoginToken,
                ErrorType = MapAuthError(result.ErrorType)
            };
        }

        private int GetSecurityInt(string key, int fallback)
        {
            return int.TryParse(_configuration[key], out var configuredValue) && configuredValue > 0
                ? configuredValue
                : fallback;
        }

        private bool GetSecurityBool(string key, bool fallback)
        {
            return bool.TryParse(_configuration[key], out var configuredValue)
                ? configuredValue
                : fallback;
        }

        private static string BuildFastLoginAttemptKey(HttpContext httpContext, string cookieValue)
        {
            var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var rawKey = $"{remoteIp}:{cookieValue}";
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));
        }

        private bool IsFastLoginLocked(string attemptKey, out int lockoutSeconds)
        {
            lockoutSeconds = 0;
            if (!FastLoginAttempts.TryGetValue(attemptKey, out var state))
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow;
            if (state.LockedUntil <= now)
            {
                FastLoginAttempts.TryRemove(attemptKey, out _);
                return false;
            }

            if (state.Count < GetSecurityInt("Security:FastLogin:MaxAttempts", 5))
            {
                return false;
            }

            lockoutSeconds = (int)Math.Ceiling((state.LockedUntil - now).TotalSeconds);
            return true;
        }

        private void RegisterFastLoginFailure(string attemptKey)
        {
            var maxAttempts = GetSecurityInt("Security:FastLogin:MaxAttempts", 5);
            var lockoutMinutes = GetSecurityInt("Security:FastLogin:LockoutMinutes", 15);
            var now = DateTimeOffset.UtcNow;

            FastLoginAttempts.AddOrUpdate(
                attemptKey,
                _ => new FastLoginAttemptState(1, now.AddMinutes(lockoutMinutes)),
                (_, current) =>
                {
                    var count = current.LockedUntil <= now ? 1 : current.Count + 1;
                    var lockedUntil = count >= maxAttempts ? now.AddMinutes(lockoutMinutes) : now.AddSeconds(30);
                    return new FastLoginAttemptState(count, lockedUntil);
                });
        }

        private static void ClearFastLoginFailures(string attemptKey)
        {
            FastLoginAttempts.TryRemove(attemptKey, out _);
        }

        private static AuthErrorType? MapAuthError(AuthFailure? failure)
        {
            return failure switch
            {
                AuthFailure.NotActive => AuthErrorType.NotActive,
                AuthFailure.InternalError => AuthErrorType.InternalError,
                _ => AuthErrorType.InvalidCredentials
            };
        }

        private sealed record FastLoginAttemptState(int Count, DateTimeOffset LockedUntil);
    }
}
