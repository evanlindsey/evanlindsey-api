using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using EvanLindseyApi.Models;
using System;

namespace EvanLindseyApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly DataContext _context;
        private AuthSettings _authSettings;

        public AuthController(DataContext context, IOptions<AuthSettings> authSettings)
        {
            _context = context;
            _authSettings = authSettings.Value;
        }

        private string CreateToken(User user)
        {
            string secret = _authSettings.SECRET;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new Claim[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) };
            var token = new JwtSecurityToken(claims: claims, signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private OkObjectResult AuthResult(User user)
        {
            return Ok(
                new
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = CreateToken(user)
                }
            );
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Get()
        {
            string id = HttpContext.User.Claims.First().Value;
            int userId = Convert.ToInt32(id);
            var user = _context.Users.SingleOrDefault(x => x.Id == userId);
            user.Password = null;
            return Ok(user);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody]User user)
        {
            string username = user.UserName;
            string password = user.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return Unauthorized();

            user = _context.Users.SingleOrDefault(x => x.UserName == username);

            if (user == null)
                return Unauthorized();

            var hasher = new PasswordHasher<User>();
            if (hasher.VerifyHashedPassword(user, user.Password, password) == 0)
                return Unauthorized();

            return AuthResult(user);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody]User user)
        {
            string username = user.UserName;
            string password = user.Password;

            if (string.IsNullOrWhiteSpace(password))
                return BadRequest("Password is required");

            if (_context.Users.Any(x => x.UserName == username))
                return BadRequest("Username " + username + " is already taken");

            var hasher = new PasswordHasher<User>();
            string hash = hasher.HashPassword(user, password);
            user.Password = hash;

            _context.Users.Add(user);
            _context.SaveChanges();

            return AuthResult(user);
        }

        [Authorize]
        [HttpPut("update")]
        public IActionResult Update([FromBody]User user)
        {
            string username = user.UserName;
            string password = user.Password;
            string firstName = user.FirstName;
            string lastName = user.LastName;

            string id = HttpContext.User.Claims.First().Value;
            int userId = Convert.ToInt32(id);
            user = _context.Users.SingleOrDefault(x => x.Id == userId);

            if (user == null)
                return BadRequest("User not found");

            if (!string.IsNullOrWhiteSpace(username))
            {
                if (user.UserName != username)
                {
                    if (_context.Users.Any(x => x.UserName == username))
                        return BadRequest("Username " + username + " is already taken");
                }
                user.UserName = username;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                var hasher = new PasswordHasher<User>();
                string hash = hasher.HashPassword(user, password);
                user.Password = hash;
            }

            user.FirstName = firstName;
            user.LastName = lastName;

            _context.Users.Update(user);
            _context.SaveChanges();

            user.Password = null;

            return Ok(user);
        }
    }
}
