using UnityEngine;
using Pandora.Network.Data;
using System;
using System.Text;

namespace Pandora.Network
{
    public class JWT
    {
        private string GetClaims(string jwt)
        {
            var sections = jwt.Split('.');
            var encodedClaims = sections[1];
            var bytesClaims = Convert.FromBase64String(encodedClaims);
            var claims = Encoding.UTF8.GetString(bytesClaims);

            return claims;
        }

        public T DecodeJwtPayload<T>(string jwt)
        {
            var claims = GetClaims(jwt);
            var decoded = JsonUtility.FromJson<GenericJwtPayload<T>>(claims);

            return decoded.payload;
        }
    }
}