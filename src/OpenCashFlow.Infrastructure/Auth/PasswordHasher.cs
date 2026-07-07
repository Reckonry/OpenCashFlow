using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;

namespace OpenCashFlow.Infrastructure.Auth
{
    public class PasswordHasher
    {
        public static string HashPassword(string clearPassword)
        {
            //byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: clearPassword, salt:
                            Encoding.ASCII.GetBytes("KFYzqeN2plZekNC88Umm1F=="),
                            prf: KeyDerivationPrf.HMACSHA256, iterationCount: 100000,
                            numBytesRequested: 256 / 8));

        }

        public static string HashPasswordV2(string clearPassword, out string salt)
        {
            // Genera un salt casuale
            byte[] saltBytes = RandomNumberGenerator.GetBytes(128 / 8); // 16 byte salt
            salt = Convert.ToBase64String(saltBytes);

            // Genera l'hash
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: clearPassword,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 256 / 8));
        }

        public static string HashPasswordArgon2(string clearPassword, out string salt)
        {
            // Genera un salt casuale
            byte[] saltBytes = RandomNumberGenerator.GetBytes(16); // 16 byte salt
            salt = Convert.ToBase64String(saltBytes);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(clearPassword)))
            {
                argon2.Salt = saltBytes;
                argon2.DegreeOfParallelism = 4; // Numero di thread
                argon2.MemorySize = 65536;     // Memoria in KB (64 MB)
                argon2.Iterations = 3;         // Iterazioni

                return Convert.ToBase64String(argon2.GetBytes(32)); // Hash di 256 bit
            }
        }

        public static string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public static string HashPasswordArgon2(string clearPassword, string salt)
        {
            using var argon2 = new Konscious.Security.Cryptography.Argon2id(Encoding.UTF8.GetBytes(clearPassword))
            {
                Salt = Convert.FromBase64String(salt),
                DegreeOfParallelism = 4,  // Numero di thread
                MemorySize = 65536,      // Memoria in KB (64 MB)
                Iterations = 3           // Iterazioni
            };

            return Convert.ToBase64String(argon2.GetBytes(32)); // Hash di 256 bit
        }
    }
}
