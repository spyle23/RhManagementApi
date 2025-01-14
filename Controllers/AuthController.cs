using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RhManagementApi.Data;
using RhManagementApi.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RhManagementApi.Model;
using System.Text;

namespace RhManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);


            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized("Invalid email or password");
            }

            AuthReturnDto auth = CreateToken(user);

            return Ok(auth);
        }

        private AuthReturnDto CreateToken(User user)
        {
            var role = "";
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName),
        new Claim("Cin", user.Cin.ToString())
    };

            // Add role-based claims
            if (user is Admin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                role = "Admin";
            }
            else if (user is RH)
            {
                claims.Add(new Claim(ClaimTypes.Role, "RH"));
                role = "RH";
            }
            else if (user is Manager)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Manager"));
                role = "Manager";
            }
            else if (user is Employee)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Employee"));
                role = "Employee";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value ??
                throw new InvalidOperationException("Token key is not configured")));

            // You can also optionally use HmacSha256Signature instead if you prefer
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new AuthReturnDto { UserId = user.Id, Role = role, Token =  new JwtSecurityTokenHandler().WriteToken(token) };
        }
    }
} 