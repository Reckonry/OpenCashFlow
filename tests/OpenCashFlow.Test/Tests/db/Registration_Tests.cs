using System.Net;
using System.Text.Json;
using OpenCashFlow.Test.Factories;
using global::Shared.DTOs;
using Xunit;

namespace OpenCashFlow.Test.Tests
{
    public class RegistrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RegistrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(); // simula client HTTP reale
        }
       
        //todo: Insert [OK] with valid Email, PasswordHash, UserName (simulate IdentityUser)
        //todo: Insert [FAIL] with Email = null or duplicate (unique constraint violation)
        //todo: Insert [FAIL] with duplicate UserName
        //todo: Insert [FAIL] with PasswordHash = null (required field)

        //todo: Insert [OK] on AspNetUsers + related insert on Company_Staff (FK between UserID and Staff)
        //todo: Insert [FAIL] if the registered user is not associated with any company 

        //todo: Insert [FAIL] Company_Staff with UserID not present in AspNetUsers (FK violation)
        //todo: Insert [FAIL] if the associated TenantID does not exist (FK violation)

        //todo: Insert [FAIL] with EmailConfirmed = true on first insert (should be false by default)
        //todo: Insert [OK] User + Claims/Role associated correctly (if managed manually)

        //todo: Update [OK] updates user profile data (FirstName, LastName, etc.)

        //todo: SoftDelete [OK] disables user (lockout or custom flag like IsActive = false and obscures data)
    }
}