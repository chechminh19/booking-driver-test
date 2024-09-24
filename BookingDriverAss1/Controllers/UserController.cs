using Application.DTO;
using Application.Service;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
        //[HttpPost("login")]
        //public async Task<IActionResult> LoginAsync(LoginUserDTO loginObject)
        //{
        //    var result = await _userService.LoginAsync(loginObject);

        //    if (!result.Success)
        //    {
        //        return StatusCode(401, result);
        //    }
        //    else
        //    {
        //        return Ok(
        //            new
        //            {
        //                success = result.Success,
        //                message = result.Message,
        //                token = result.DataToken,
        //                role = result.Role,
        //            }
        //        );
        //    }
        //}
    }
}
