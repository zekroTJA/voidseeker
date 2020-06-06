using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RESTAPI.Extensions;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RESTAPI.Authorization
{
    /// <summary>
    /// Provides Authoriation functionalities using
    /// Jason Web Tokens.
    /// </summary>
    public class JWTAuthorization : IAuthorization
    {
        private static readonly string issuer = "voidsearch API";
        private static readonly int keyLen = 32;
        private static readonly string securityAlgorithm = SecurityAlgorithms.HmacSha256Signature;

        private SymmetricSecurityKey signingKey;
        private readonly JwtSecurityTokenHandler tokenHandler;

        /// <summary>
        /// Cretaes a new instance of JWTAuthorization using either
        /// the passed key as JWT signing key or, if it is null, 
        /// generating a random key on startup used to signing JWTs.
        /// </summary>
        public JWTAuthorization(IConfiguration configuration)
        {
            var key = configuration.GetValue<string>("WebServer:Auth:JWTSecret");

            signingKey = key.NullOrEmpty()
                ? GenerateSigningKey(keyLen)
                : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            tokenHandler = new JwtSecurityTokenHandler();
        }

        public string GetSessionKey(AuthClaims properties, TimeSpan expires)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, properties.UserId.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Name, properties.UserName));

            var credentials = new SigningCredentials(signingKey, securityAlgorithm);

            var token = tokenHandler.CreateJwtSecurityToken(
                  issuer: issuer, 
                  subject: claims, 
                  notBefore: DateTime.UtcNow, 
                  expires: DateTime.UtcNow.Add(expires), 
                  signingCredentials: credentials);

            return tokenHandler.WriteToken(token);
        }

        public AuthClaims ValidateSessionKey(string key)
        {
            var parameters = new TokenValidationParameters
            {
                IssuerSigningKey = signingKey,
                ValidIssuer = issuer,
                ValidateIssuer = true,
                ValidateAudience = false
            };

            var principal = tokenHandler.ValidateToken(key, parameters, out var _);
            var claims = principal.Identities.First().Claims;

            var userId = claims?.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userName = claims?.First(c => c.Type == ClaimTypes.Name)?.Value;

            if (userId == null || userName == null)
                throw new Exception("invalid claims");

            return new AuthClaims
            {
                UserId = Guid.Parse(userId),
                UserName = userName
            };
        }

        /// <summary>
        /// Generates a cryptographically random array
        /// of bytes with the length of len and returns
        /// it as SymmetricSecurityKey.
        /// </summary>
        /// <param name="len">key length</param>
        /// <returns>key</returns>
        private SymmetricSecurityKey GenerateSigningKey(int len)
        {
            var bytes = new byte[len];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(bytes);
            return new SymmetricSecurityKey(bytes);
        }
    }

}
