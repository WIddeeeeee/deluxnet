using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DeluxeNET.Security
{
    public class jwt
    {
        // FIX: key comes from config ("Jwt:Key") or the DELUXE_JWT_KEY env var.
        // HMAC-SHA256 needs at least 128 bits; anything shorter throws at runtime.
        // If nothing is configured, a random 256-bit key is generated (tokens won't
        // survive a restart in that case — set a real key for production).
        private readonly string _key;

        public jwt(IConfiguration config)
        {
            var configured = config["Jwt:Key"] ?? Environment.GetEnvironmentVariable("DELUXE_JWT_KEY");
            if (!string.IsNullOrWhiteSpace(configured) && Encoding.UTF8.GetByteCount(configured) >= 16)
            {
                _key = configured;
            }
            else
            {
                var bytes = RandomNumberGenerator.GetBytes(32);
                _key = Convert.ToBase64String(bytes);
                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[jwt] WARNING: no Jwt:Key configured — generated a random key. " +
                                      "Set Jwt:Key in appsettings.json or the DELUXE_JWT_KEY env var.");
                    Console.ResetColor();
                }
                catch
                {
                    Console.WriteLine("[jwt] WARNING: no Jwt:Key configured — generated a random key.");
                }
            }
        }

        public string GenerateToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            var roles = new List<string>
    {
        "gameClient",
        "moderator",
        "screenshare",
        "keepsake",
        "developer"
    };

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId)
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public long? VerifyToken(string token)
        {


            if (string.IsNullOrEmpty(token)) return null;

            token = token.Trim();

            if (token.StartsWith("Bearer "))
                token = token.Substring("Bearer ".Length).Trim();


            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_key);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                ClaimsPrincipal principal = tokenHandler.ValidateToken(
                    token, validationParameters, out SecurityToken validatedToken
                    );

                var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (long.TryParse(subClaim, out long playerId)) return playerId;

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

    }
}
