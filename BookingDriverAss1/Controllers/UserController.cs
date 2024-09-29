using Application.DTO;
using Application.Service;
using Infrastructure.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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
        /// <summary>
        /// Login into Ubersystem
        /// </summary>
        /// <param name="loginObject"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<string>>> LoginAsync(LoginUserDTO loginObject)
        {
            var result = await _userService.LoginAsync(loginObject);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new ApiResponse<string>
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Message = result.Message,
                    Data = null
                });
            }
            else
            {
                return Ok(
                   new ApiResponse<string>
                   {
                       StatusCode = HttpStatusCode.OK,
                       Message = "Login successful",
                       Data = result.Data
                   }
                );
            }
        }

        /// <summary>
        /// Register new customer or driver into Ubersystem
        /// </summary>
        /// <param name="registerObject"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<RegisterDTO>>> Register(RegisterDTO registerObject)
        {
            var result = await _userService.RegisterAsync(registerObject);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(new ApiResponse<RegisterDTO>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = result.Message,
                    Data = result.Data
                }),
                HttpStatusCode.Conflict => Conflict(new ApiResponse<RegisterDTO>
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.BadRequest => BadRequest(new ApiResponse<RegisterDTO>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<RegisterDTO>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = result.Message,
                    Data = null
                }),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }
        /// <summary>
        /// GetUserProfile Ubersystem
        /// </summary>
        /// <param name="idUser"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        /// <summary>
        /// Update profile user Ubersystem
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateUserDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(long id, UpdateUserDTO updateUserDTO)
        {
            if (id != updateUserDTO.Id)
            {
                return BadRequest(new ApiResponse<UpdateUserDTO>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "ID mismatch",
                    Data = null
                });
            }
            var result = await _userService.UpdateUserAsync(updateUserDTO);
            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(new ApiResponse<UpdateUserDTO>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = result.Message,
                    Data = result.Data
                }),
                HttpStatusCode.Conflict => Conflict(new ApiResponse<UpdateUserDTO>
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.NotFound => NotFound(new ApiResponse<UpdateUserDTO>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<UpdateUserDTO>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = result.Message,
                    Data = null
                }),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }
        /// <summary>
        /// Delete profile user Ubersystem
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(new ApiResponse<string>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = result.Message,
                    Data = result.Data
                }),
                HttpStatusCode.NotFound => NotFound(new ApiResponse<string>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = result.Message,
                    Data = null
                }),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }
       
    }
}
