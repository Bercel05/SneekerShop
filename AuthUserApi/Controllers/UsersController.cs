using AuthUserApi.Models;
using AuthUserApi.Models.DTOs;
using AuthUserApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuthUserApi.Controllers
{
    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AuthuserContext _context;
        private readonly PasswordHasher _password;

        public UsersController(AuthuserContext context, PasswordHasher password)
        {
            _context = context;
            _password = password;
        }

        [HttpPost("newUser")]
        public async Task<IActionResult> NewUser([FromBody] NewUserDto newUserDto)
        {
            try
            {
                var (pass, salt) = _password.HashPassword(newUserDto.Password);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = newUserDto.Name,
                    Email = newUserDto.Email,
                    Password = pass,
                    Salt = salt,
                    RegTime = DateTime.Now
                };

                if (user is not null)
                {
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();

                    return StatusCode(201, new { message = "Felvéve", result = user });
                }

                return StatusCode(400, new { message = "Hiba", result = user });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "teacher")]
        [HttpGet("allUser")]
        public async Task<IActionResult> AllUser()
        {
            try
            {
                var users = await _context.Users.ToListAsync();

                if (users is not null)
                {
                    return StatusCode(200, new { message = "Lekérdezve", result = users });
                }

                return StatusCode(400, new { message = "Hiba", result = users });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("byId")]
        public async Task<IActionResult> UserById([FromQuery] Guid id)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

                if (user is not null)
                {
                    return StatusCode(200, new { message = "Lekérdezve", result = user });
                }

                return StatusCode(404, new { message = "Nincs találat", result = user });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPut("updateUser")]
        public async Task<IActionResult> UpdateUser([FromQuery] Guid id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user is not null)
                {
                    user.Name = updateUserDto.Name;
                    user.Email = updateUserDto.Email;
                    user.Password = updateUserDto.Password;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    return StatusCode(200, new { message = "Frissítve", result = user });
                }

                return StatusCode(404, new { message = "Nincs találat", result = user });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser([FromQuery] Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user is not null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

                    return StatusCode(200, new { message = "Tötrölve", result = user.Name });
                }

                return StatusCode(404, new { message = "Nincs találat", result = user });

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            try
            {
                var login = await _context.Users.SingleOrDefaultAsync(u => u.Name == userLoginDto.Name);

                if (login is null)
                {
                    return StatusCode(404, new { message = "Nincs találat" });
                }

                var isValid = _password.Verify(userLoginDto.Password!, login.Password!, login.Salt!);

                if (isValid)
                {
                    return StatusCode(200, new { message = "Sikeres bejelentkezés", result = login });
                }

                return StatusCode(401, new { message = "Hibás név vagy jelszó" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}