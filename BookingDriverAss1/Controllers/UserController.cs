using Application.DTO;
using Application.Service;
using Infrastructure.DTO;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static Azure.Core.HttpHeader;

namespace BookingDriverAss1.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class UserController : BaseController
    {   private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginUserDTO loginObject)
        {
            var result = await _userService.LoginAsync(loginObject);

            if (!result.Success)
            {
                return StatusCode(401, result);
            }
            else
            {
                return Ok(
                    new
                    {
                        success = result.Success,
                        message = result.Message,
                        token = result.DataToken,
                        role = result.Role,
                    }
                );
            }
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerObject)
        {
            var result = await _userService.RegisterAsync(registerObject);

            if (!result.Success)
            {
                return BadRequest(result);
            }
            else
            {
                return Ok(result);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetUserInFor(long idUser)
        {
            var result = await _userService.GetUserByIdAsync(idUser);

            if (result == null)
            {
                return NotFound(new { Message = "User not found" });
            }
            else
            {
                return Ok(result);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, UpdateUserDTO updateUserDTO)
        {
            if (id != updateUserDTO.Id)
            {
                return BadRequest(new { Message = "ID mismatch" });
            }
            var result = await _userService.UpdateUserAsync(updateUserDTO);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }



    }
}
