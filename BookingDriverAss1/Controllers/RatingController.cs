using Application.Service;
using Infrastructure.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BookingDriverAss1.Controllers
{
    public class RatingController : BaseController
    {
        private readonly RatingService _rateService;
        public RatingController(RatingService rateService)
        {
            _rateService = rateService;
        }
        /// <summary>
        /// customer rates driver when complete trip
        /// </summary>
        /// <param name="ratingDto"></param>
        /// <returns></returns>
        [HttpPost("rate-driver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<RatingDTO>>> RateDriver(RatingDTO ratingDto)
        {
            var result = await _rateService.RateDriverAsync(ratingDto);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(new ApiResponse<RatingDTO>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = result.Message,
                    Data = result.Data
                }),
                HttpStatusCode.NotFound => NotFound(new ApiResponse<RatingDTO>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.BadRequest => BadRequest(new ApiResponse<RatingDTO>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<RatingDTO>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = result.Message,
                    Data = null
                }),
                _ => StatusCode((int)result.StatusCode, new ApiResponse<RatingDTO>
                {
                    StatusCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                })
            };
        }
    }
}
