using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlueProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginDto loginDto)
        {
            // Validate user credentials (this is just a placeholder; implement your own user validation logic)
            if (loginDto.Username == "test" && loginDto.Password == "password") // Example credentials
            {
                var claims = new[]
                {
                new Claim(ClaimTypes.Name, loginDto.Username)
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])),
                    signingCredentials: creds);

                return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized(); // Return 401 if login fails
        }

        public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }


    }
}
