using FW.WAPI.Core.DAL.Model.Jwt;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FW.WAPI.Core.General
{
    public static class JwtUtilities
    {
        /// <summary>
        /// Create JWT payload
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="tenantCode"></param>
        /// <param name="role"></param>
        /// <param name="secretKey"></param>
        /// <param name="expireMinutes"></param>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public static string CreateJwt(string userName, string tenantCode, string role,
            Audience audience, int? expireMinute = null, string session = null)
        {
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(TokenConst.TENANT_CODE, tenantCode),
                new Claim(ClaimTypes.Role, role),
                new Claim(TokenConst.USER_NAME, userName),
            };

            if (!string.IsNullOrEmpty(session))
            {
                claims[3] = new Claim(TokenConst.SESSION, session);
            }

            var symmetricKeyAsBase64 = audience.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            JwtSecurityToken jwt;

            if (expireMinute.HasValue)
            {
                jwt = new JwtSecurityToken(
                issuer: audience.Iss,
                audience: audience.Aud,
                claims: claims,
                notBefore: now,
                //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
                expires: now.Add(TimeSpan.FromMinutes(expireMinute.Value)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }
            else
            {
                jwt = new JwtSecurityToken(
               issuer: audience.Iss,
               audience: audience.Aud,
               claims: claims,
               notBefore: now,
               //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
               expires: now.Add(TimeSpan.FromMinutes(audience.ExpireMinutes)),
               signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="tenantCode"></param>
        /// <param name="roles"></param>
        /// <param name="audience"></param>
        /// <param name="expireMinute"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string CreateJwt(string userName, string tenantCode, string[] roles,
           Audience audience, int? expireMinute = null, string session = null)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(TokenConst.TENANT_CODE, tenantCode),
                new Claim(TokenConst.USER_NAME, userName),
            };

            foreach (var item in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }

            if (!string.IsNullOrEmpty(session))
            {
                claims.Add(new Claim(TokenConst.SESSION, session));
            }

            var symmetricKeyAsBase64 = audience.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            JwtSecurityToken jwt;

            if (expireMinute.HasValue)
            {
                jwt = new JwtSecurityToken(
                issuer: audience.Iss,
                audience: audience.Aud,
                claims: claims,
                notBefore: now,
                //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
                expires: now.Add(TimeSpan.FromMinutes(expireMinute.Value)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }
            else
            {
                jwt = new JwtSecurityToken(
               issuer: audience.Iss,
               audience: audience.Aud,
               claims: claims,
               notBefore: now,
               //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
               expires: now.Add(TimeSpan.FromMinutes(audience.ExpireMinutes)),
               signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="tenantCode"></param>
        /// <param name="audience"></param>
        /// <param name="expireMinute"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string CreateJwt(string userName, string tenantCode,
           Audience audience, int? expireMinute = null, string session = null)
        {
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(TokenConst.TENANT_CODE, tenantCode),
                new Claim(TokenConst.USER_NAME, userName),
            };

            if (!string.IsNullOrEmpty(session))
            {
                claims[2] = new Claim(TokenConst.SESSION, session);
            }

            var symmetricKeyAsBase64 = audience.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            JwtSecurityToken jwt;

            if (expireMinute.HasValue)
            {
                jwt = new JwtSecurityToken(
                issuer: audience.Iss,
                audience: audience.Aud,
                claims: claims,
                notBefore: now,
                //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
                expires: now.Add(TimeSpan.FromMinutes(expireMinute.Value)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }
            else
            {
                jwt = new JwtSecurityToken(
               issuer: audience.Iss,
               audience: audience.Aud,
               claims: claims,
               notBefore: now,
               //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
               expires: now.Add(TimeSpan.FromMinutes(audience.ExpireMinutes)),
               signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// Create Jwt without tenant
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="role"></param>
        /// <param name="secretKey"></param>
        /// <param name="expireMinutes"></param>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public static string CreateJwt(string userName, string role,
          Audience audience)
        {
            var now = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(TokenConst.USER_NAME, userName),
            };

            var symmetricKeyAsBase64 = audience.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var jwt = new JwtSecurityToken(
                issuer: audience.Iss,
                audience: audience.Aud,
                claims: claims,
                notBefore: now,
                //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
                expires: now.Add(TimeSpan.FromMinutes(audience.ExpireMinutes)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roles"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public static string CreateJwt(string userName, string[] roles,
            Audience audience)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>()
            {
                new Claim(TokenConst.USER_NAME, userName),
            };

            foreach (var item in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }

            var symmetricKeyAsBase64 = audience.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var jwt = new JwtSecurityToken(
                issuer: audience.Iss,
                audience: audience.Aud,
                claims: claims,
                notBefore: now,
                //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
                expires: now.Add(TimeSpan.FromMinutes(audience.ExpireMinutes)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// Create Jwt with tenant
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roles"></param>
        /// <param name="tenantCodes"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public static string CreateJwt(string userName, string[] roles, string tenantCodes,
            Audience audience)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>()
            {
                new Claim(TokenConst.USER_NAME, userName),
            };

            if (tenantCodes != null)
            {     
                claims.Add(new Claim(TokenConst.TENANT_CODES, tenantCodes));
            }

            foreach (var item in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }

            var symmetricKeyAsBase64 = audience.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            var jwt = new JwtSecurityToken(
                issuer: audience.Iss,
                audience: audience.Aud,
                claims: claims,
                notBefore: now,
                //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
                expires: now.Add(TimeSpan.FromMinutes(audience.ExpireMinutes)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// Create JWT payload
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="tenantCode"></param>
        /// <param name="role"></param>
        /// <param name="companyCode"></param>
        /// <param name="audience"></param>
        /// <param name="expireMinute"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string CreateJwt(string userName, string tenantCode, string role, string companyCode,
            Audience audience, int? expireMinute = null, string session = null)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(TokenConst.TENANT_CODE, tenantCode),
                new Claim(ClaimTypes.Role, role),
                new Claim(TokenConst.USER_NAME, userName),
                new Claim(TokenConst.COMPANY_CODE, companyCode)
            };

            if (!string.IsNullOrEmpty(session))
            {
                claims.Add(new Claim(TokenConst.SESSION, session));
            }

            var symmetricKeyAsBase64 = audience.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            JwtSecurityToken jwt;

            if (expireMinute.HasValue)
            {
                jwt = new JwtSecurityToken(
                issuer: audience.Iss,
                audience: audience.Aud,
                claims: claims,
                notBefore: now,
                //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
                expires: now.Add(TimeSpan.FromMinutes(expireMinute.Value)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }
            else
            {
                jwt = new JwtSecurityToken(
               issuer: audience.Iss,
               audience: audience.Aud,
               claims: claims,
               notBefore: now,
               //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
               expires: now.Add(TimeSpan.FromMinutes(audience.ExpireMinutes)),
               signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// Create JWT payload
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="audience"></param>
        /// <param name="expireMinute"></param>
        /// <returns></returns>
        public static string CreateJwt(List<Claim> claims, Audience audience, int? expireMinute = null)
        {
            var now = DateTime.UtcNow;

            var symmetricKeyAsBase64 = audience.Secret;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey(keyByteArray);

            JwtSecurityToken jwt;

            if (expireMinute.HasValue)
            {
                jwt = new JwtSecurityToken(
                issuer: audience.Iss,
                audience: audience.Aud,
                claims: claims,
                notBefore: now,
                //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
                expires: now.Add(TimeSpan.FromMinutes(expireMinute.Value)),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }
            else
            {
                jwt = new JwtSecurityToken(
               issuer: audience.Iss,
               audience: audience.Aud,
               claims: claims,
               notBefore: now,
               //expires: now.Add(TimeSpan.FromMinutes(_audSettings.ExpireMinutes)),
               expires: now.Add(TimeSpan.FromMinutes(audience.ExpireMinutes)),
               signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            }

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// Get Principal From Expired Token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token, Audience audience)
        {
            var keyByteArray = Encoding.ASCII.GetBytes(audience.Secret);
            var signingKey = new SymmetricSecurityKey(keyByteArray);
            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT issuer (Iss) claim
                ValidateIssuer = true,
                ValidIssuer = audience.Iss,

                // Validate the JWT audience (Aud) claim
                ValidateAudience = true,
                ValidAudience = audience.Aud,

                // Validate token expiration
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
