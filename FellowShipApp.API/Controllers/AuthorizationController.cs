using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FellowShipApp.API.Data;
using FellowShipApp.API.Dtos;
using FellowShipApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FellowShipApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthorizationController : ControllerBase
    {
        readonly IAuthRepository repo;
        IConfiguration config;
        public AuthorizationController(IAuthRepository repo, IConfiguration config)
        {
            this.repo = repo;
            this.config = config;
        }
        // GET: api/<AuthorizationController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<AuthorizationController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login ([FromBody] UserForLogin login)
        {
            var userRepo = await repo.Login(login.Username.ToLower(), login.Password);

            if (userRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userRepo.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var lToken = tokenHandler.CreateToken(tokenDesc);

            return Ok(new
            {
                Token = tokenHandler.WriteToken(lToken)
            }
            );
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto registerDto)
        {
            //Validate the request

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await repo.UserExists(registerDto.Username.ToLower()))
            {
                return BadRequest("User Name is already Exists");
            }

            var user = new User()
            {
                UserName = registerDto.Username

            };

            Console.WriteLine(registerDto.Username + "-" + registerDto.Password);
            var createdUser = await repo.Register(user, registerDto.Password);

            return StatusCode(201);
        }

        // PUT api/<AuthorizationController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AuthorizationController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
