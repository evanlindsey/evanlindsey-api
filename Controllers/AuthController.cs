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

        private string CreateToken(User u)
        {
            string secret = _authSettings.SECRET;
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var claims = new Claim[] { new Claim(JwtRegisteredClaimNames.Sub, u.Id.ToString()) };
            var jwt = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private OkObjectResult AuthResult(User u)
        {
            string token = CreateToken(u);

            return Ok(
                new
                {
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Token = token
                }
            );
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Get()
        {
            string id = HttpContext.User.Claims.First().Value;
            User user = _context.Users.SingleOrDefault(u => u.Id.ToString() == id);

            user.Password = null;

            return Ok(user);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody]User u)
        {
            string username = u.UserName;
            string password = u.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return Unauthorized();

            User user = _context.Users.SingleOrDefault(x => x.UserName == username);

            if (user == null)
                return Unauthorized();

            var passwordHasher = new PasswordHasher<User>();
            if (passwordHasher.VerifyHashedPassword(user, user.Password, password) == 0)
                return Unauthorized();

            return AuthResult(user);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody]User u)
        {
            string username = u.UserName;
            string password = u.Password;

            if (string.IsNullOrWhiteSpace(password))
                return BadRequest("Password is required");

            if (_context.Users.Any(x => x.UserName == username))
                return BadRequest("Username " + username + " is already taken");

            User user = u;

            var passwordHasher = new PasswordHasher<User>();
            string hash = passwordHasher.HashPassword(user, password);
            user.Password = hash;

            _context.Users.Add(user);
            _context.SaveChanges();

            return AuthResult(user);
        }

        [Authorize]
        [HttpPut("update")]
        public IActionResult Update([FromBody]User u)
        {
            string id = HttpContext.User.Claims.First().Value;
            User user = _context.Users.SingleOrDefault(x => x.Id.ToString() == id);

            if (user == null)
                return BadRequest("User not found");

            string username = u.UserName;
            string password = u.Password;
            string firstname = u.FirstName;
            string lastname = u.LastName;

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
                var passwordHasher = new PasswordHasher<User>();
                string hash = passwordHasher.HashPassword(user, password);

                user.Password = hash;
            }

            user.FirstName = firstname;
            user.LastName = lastname;

            _context.Users.Update(user);
            _context.SaveChanges();

            user.Password = null;

            return Ok(user);
        }
    }
}
