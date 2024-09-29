using Application.AppConfig;
using AutoMapper;
using Infrastructure;
using Infrastructure.DTO;
using Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class RatingService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly AppConfiguration _config;
        private readonly IMapper _mapper;
        public RatingService(UnitOfWork unitOfWork, AppConfiguration configuration, IMapper mapper)
        {
            _config = configuration;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ApiResponse<RatingDTO>> RateDriverAsync(RatingDTO ratingDto)
        {
            var response = new ApiResponse<RatingDTO>();

            try
            {
                // Check if the customer not found
                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(ratingDto.CustomerId);
                if (customer == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Customer not found";
                    return response;
                }
                // Check if the driver not found
                var driver = await _unitOfWork.DriverRepository.GetByIdAsync(ratingDto.DriverId);
                if (driver == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Driver not found";
                    return response;
                }
                // Check if the trip not found
                var trip = await _unitOfWork.TripRepository.GetByIdAsync(ratingDto.TripId.Value);
                if (trip == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Trip not found";
                    return response;
                }
                // Check if the trip has already been rated
                var existingRating = await _unitOfWork.RatingRepository.GetRatingByTripIdAsync(ratingDto.TripId.Value);
                if (existingRating != null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "This trip has already been rated.";
                    return response;
                }
                // Check if the trip not yet completed
                if (trip.Status == TripStatus.O.ToString())
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "Cannot rate a driver for an incomplete trip.";
                    return response;
                }
                else if(trip.Status == TripStatus.C.ToString())
                {
                    string feedbackValue = ratingDto.Feedback; // Assuming this is a string input from the user
                                                               // Validate the feedback input (1 to 5)
                    if (!int.TryParse(feedbackValue, out int feedbackNumber) || feedbackNumber < 1 || feedbackNumber > 5)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.Message = "Feedback must be between 1 and 5.";
                        return response;
                    }
                    // Create a new rating object
                    var rating = new Rating
                    {
                        Id = await _unitOfWork.RatingRepository.GetMaxRatingIdAsync() + 1, // Set ID based on max
                        CustomerId = ratingDto.CustomerId,
                        DriverId = ratingDto.DriverId,
                        TripId = ratingDto.TripId,
                        Rating1 = ratingDto.Rating1,
                        Feedback = ratingDto.Feedback.ToString(),
                    };

                    await _unitOfWork.RatingRepository.AddAsync(rating);                  
                    await _unitOfWork.SaveChangeAsync();

                    // Update the trip status to Rated (R)
                    trip.Status = TripStatus.R.ToString();
                    await _unitOfWork.TripRepository.UpdateTrip(trip); // Make sure you have an UpdateTrip method
                    await _unitOfWork.SaveChangeAsync();
                    
                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "Rating submitted successfully";
                    response.Data = _mapper.Map<RatingDTO>(rating);
                }
            }
            catch (DbException ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Database error occurred.";
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An error occurred.";
            }
            return response;
        }
    }
}
