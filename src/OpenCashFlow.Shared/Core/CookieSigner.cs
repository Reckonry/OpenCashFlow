using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core
{
    public static class CookieSigner
    {
        /// <summary>
        /// Calcola HMAC‐SHA256 di “companyId” (stringa UTF8) usando secretKey (Base64 URL) come chiave
        /// </summary>
        //todo: portare a private...
        private static string ComputeHMAC(string companyId, string secretKeyBase64Url)
        {
            // 1) Converti il secretKey Base64 URL‐safe in byte[]
            var padded = secretKeyBase64Url.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
                    // se modulo 0, non serve padding
            }
            var keyBytes = Convert.FromBase64String(padded);

            // 2) Calcola HMAC‐SHA256
            using var hmac = new HMACSHA256(keyBytes);
            var dataBytes = Encoding.UTF8.GetBytes(companyId);
            var hashBytes = hmac.ComputeHash(dataBytes);

            // 3) Restituisci in Base64 URL‐safe (senza padding)
            var signature = Convert.ToBase64String(hashBytes)
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return signature;
        }

        public static string ProtectCompanyCookie(Guid companyId, string secretKeyBase64Url)
        {
            // 1) Componi la stringa in chiaro: "CompanyID.Signature"
            var companyIdString = companyId.ToString(); // es. "41bc4db9-89c5-44e9-978c-988d0ff8f5de"
            var signature = ComputeHMAC(companyIdString, secretKeyBase64Url);
            var payload = $"{companyIdString}.{signature}";

            //// 2) "Protect" il payload con Data Protection (cifra+firma)
            //var protectedPayload = _protector.Protect(payload);
            //return protectedPayload;
            return payload; // Per ora restituiamo solo il payload, senza protezione aggiuntiva
        }

        public static (Guid? CompanyID, bool IsValid) UnprotectCompanyCookie(string payload)
        {
            try
            {
                // 1) "Unprotect" -> ottieni il payload in chiaro "CompanyID.Signature"
                //var payload = _protector.Unprotect(protectedCookieValue);

                // 2) Split tra CompanyID e Signature
                var parts = payload.Split('.', 2);
                if (parts.Length != 2)
                    return (null, false);

                var companyIdString = parts[0];
                var signatureInCookie = parts[1];

                if (!Guid.TryParse(companyIdString, out var companyId))
                    return (null, false);

                // 3) Ritorniamo solo il CompanyID: 
                //    la verifica HMAC la farai poi (o la fai qui ricalcolando con la SecretKey)
                return (companyId, true);
            }
            catch
            {
                // Se Unprotect lancia eccezione (cifratura corrotta o firma non valida), restituiamo false
                return (null, false);
            }
        }
    }
}
