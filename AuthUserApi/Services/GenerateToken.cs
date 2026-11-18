using AuthUserApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthUserApi.Services
{
    public class GenerateToken
    {
        public string Token(Guid Id, string email, ICollection<Role> Role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ide-jon-az-a-kulcs-amivel-azonositjuk-issuer-es-a-audience");
            var claimlist = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
            };
            foreach (var r  in Role)
            {
                claimlist.Add(new Claim(ClaimTypes.Role, r.Name));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = "auth-client",
                Issuer = "auth-api",
                Subject = new ClaimsIdentity(claimlist),
                Expires = DateTime.UtcNow.AddMinutes(120),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
