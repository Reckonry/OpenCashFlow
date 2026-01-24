using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core
{
    public class SecretKeyGenerator
    {
        public static string GenerateSecretKey(int byteLength = 64)
        {
            var buffer = new byte[byteLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            // Base64 “URL‐safe” senza padding finale (“=”)
            return Convert.ToBase64String(buffer).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
