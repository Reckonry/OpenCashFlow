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
       
        //todo: Insert [OK] con Email, PasswordHash, UserName validi (simulate IdentityUser)
        //todo: Insert [FAIL] con Email = null o duplicata (violazione unique)
        //todo: Insert [FAIL] con UserName duplicato
        //todo: Insert [FAIL] con PasswordHash = null (campo obbligatorio)

        //todo: Insert [OK] su AspNetUsers + insert collegato su Company_Staff (FK tra UserID e Staff)
        //todo: Insert [FAIL] se user registrato non è associato a nessuna company 

        //todo: Insert [FAIL] Company_Staff con UserID non presente in AspNetUsers (violazione FK)
        //todo: Insert [FAIL] se TenantID associata non esiste (violazione FK)

        //todo: Insert [FAIL] con flag EmailConfirmed = true al primo inserimento (se deve essere false by default)
        //todo: Insert [OK] User + Claims/Role associati correttamente (se li gestisci manualmente)

        //todo: Update [OK] modifica dati anagrafici utente (FirstName, LastName, ecc.)

        //todo: SoftDelete [OK] disattiva utente (lockout o flag custom tipo IsActive = false e oscura dati)
    }
}