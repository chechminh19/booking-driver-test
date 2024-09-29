using Application.Service;
using Infrastructure.DTO;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BookingDriverAss1.Controllers
{
    [Route("api/trip")]
    [ApiController]
    public class TripController: BaseController
    {
        private readonly TripService _tripService;
        public TripController(TripService tripService)
        {
            _tripService = tripService;
        }
        /// <summary>
        /// Create trip, system response drivers not on trip within 2kms
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="startLat"></param>
        /// <param name="startLng"></param>
        /// <param name="endLat"></param>
        /// <param name="endLng"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<Driver>>>> CreateTrip(
           long customerId,
           double startLat,
           double startLng,
           double endLat,
           double endLng)
        {
            var result = await _tripService.CreateTrip(customerId, startLat, startLng, endLat, endLng);
            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(new ApiResponse<List<Driver>>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = result.Message,
                    Data = result.Data
                }),
                HttpStatusCode.NotFound => NotFound(new ApiResponse<List<Driver>>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<Driver>>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = result.Message,
                    Data = null
                }),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }
        /// <summary>
        /// driver confirm trip from customer
        /// </summary>
        /// <param name="confirmTripDto"></param>
        /// <returns></returns>
        [HttpPost("confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Trip>>> ConfirmPickup([FromBody] ConfirmTripDTO confirmTripDto)
        {
            var result = await _tripService.ConfirmPickupAsync(
                    confirmTripDto.CustomerId,
                    confirmTripDto.DriverId,
                    confirmTripDto.StartLat,
                    confirmTripDto.StartLng,
                    confirmTripDto.EndLat,
                    confirmTripDto.EndLng);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(new ApiResponse<Trip>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = result.Message,
                    Data = result.Data
                }),
                HttpStatusCode.NotFound => NotFound(new ApiResponse<Trip>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<Trip>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = result.Message,
                    Data = null
                }),
                _ => StatusCode((int)result.StatusCode, result)
            };
        }
        /// <summary>
        /// driver complete trip
        /// </summary>
        /// <param name="tripId"></param>
        /// <param name="driverId"></param>
        /// <returns></returns>
        [HttpPost("complete/{tripId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Trip>>> CompleteTrip(long tripId, long driverId)
        {
            var result = await _tripService.CompleteTripAsync(tripId, driverId);

            return result.StatusCode switch
            {
                HttpStatusCode.OK => Ok(new ApiResponse<Trip>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = result.Message,
                    Data = result.Data
                }),
                HttpStatusCode.NotFound => NotFound(new ApiResponse<Trip>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = result.Message,
                    Data = null
                }),
                HttpStatusCode.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<Trip>
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
