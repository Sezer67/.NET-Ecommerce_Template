using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.UserService.Model;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace ECommerce.UserService.Service
{
    public class AuthService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _config;

        public AuthService(IConnectionMultiplexer redis, IConfiguration config)
        {
            _redis = redis;
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var keyString = jwtSettings["Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new ArgumentNullException("Jwt:Key", "JWT key cannot be null or empty.");
            }
            var key = Encoding.ASCII.GetBytes(keyString);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username)
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);


            var tokenString = tokenHandler.WriteToken(token);

            var db = _redis.GetDatabase();
            db.StringSet($"token:{user.Id}", tokenString, TimeSpan.FromHours(1));

            return tokenString;
        }
    }
}