using UnityEngine;
using Pandora.Network.Data;
using System;
using System.Text;

namespace Pandora.Network
{
    public class JWT
    {
        /// <summary>
        /// Sanitize an unsafe base64 string.
        /// (This function will replace invalid characters and
        /// add a padding using '=')
        /// </summary>
        /// <param name="base64Encoded">A non-sanitized base64 string</param>
        /// <returns>A sanitized base64 string</returns>
        private string SanitizeBase64String(string base64Encoded)
        {
            var sanitized = base64Encoded.Replace('_', '/').Replace('-', '+');

            switch (sanitized.Length % 4)
            {
                case 2: sanitized += "=="; break;
                case 3: sanitized += "="; break;
            }

            return sanitized;
        }

        /// <summary>
        /// Get the claims from a JWT token
        /// </summary>
        /// <param name="jwt">The JWT string</param>
        /// <returns>The decoded JWT payload</returns>
        private string GetClaims(string jwt)
        {
            var sections = jwt.Split('.');
            var encodedClaims = sections[1];
            var sanitizedEncodedClaims = SanitizeBase64String(encodedClaims);
            var bytesClaims = Convert.FromBase64String(sanitizedEncodedClaims);
            var claims = Encoding.UTF8.GetString(bytesClaims);

            return claims;
        }

        /// <summary>
        /// Decode the JWT JSON payload.
        /// This method does not handle validation.
        /// </summary>
        /// <param name="jwt">The jwt string</param>
        /// <typeparam name="T">The payload structure to deserialize</typeparam>
        /// <returns></returns>
        public T DecodeJwtPayload<T>(string jwt)
        {
            var claims = GetClaims(jwt);
            var decoded = JsonUtility.FromJson<GenericJwtPayload<T>>(claims);

            return decoded.payload;
        }
    }
}