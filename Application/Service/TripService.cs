using Application.ServiceResponse;
using Infrastructure;
using Infrastructure.DTO;
using Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class TripService
    {
        private readonly UberSystemContext _systemContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly UnitOfWork _unitOfWork; 
        public DateTime CreateAt { get; set; }
        public TripService(UberSystemContext systemContext, IServiceProvider serviceProvider, UnitOfWork unitOfWork)
        {
            _systemContext = systemContext;
            _serviceProvider = serviceProvider;
            _unitOfWork = unitOfWork;
            CreateAt = DateTime.Now;
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // Earth radius in meters
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c / 1000; // Convert to kilometers
        }
        public async Task<List<Driver>> GetAvailableDriversAsync(double pickupLatitude, double pickupLongitude)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                // Get all drivers who have no current trip (Status = "Available" or similar)
                var availableDrivers = await unitOfWork.DriverRepository.GetAvailableDrivers();

                // Filter drivers within 2 KM of the pickup location
                var driversWithinRadius = availableDrivers
                    .Where(driver =>
                        driver.LocationLatitude.HasValue &&
                        driver.LocationLongitude.HasValue &&
                        CalculateDistance(pickupLatitude, pickupLongitude,
                            driver.LocationLatitude.Value, driver.LocationLongitude.Value) <= 2)
                    .ToList();

                var selectedDriver = driversWithinRadius
                .OrderByDescending(driver => driver.Ratings.Any() ? driver.Ratings.Average(r => r.Rating1 ?? 0) : 0)
                        .FirstOrDefault();
                return selectedDriver != null ? new List<Driver> { selectedDriver } : new List<Driver>();

            }
        }
        public async Task<ApiResponse<List<Driver>>> CreateTrip(long customerId, double startLat, double startLng, double endLat, double endLng)
        {
            var response = new ApiResponse<List<Driver>>();
            try
            {   // Check if Customer exists
                var customerExists = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                if (customerExists == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Customer not found.";
                    return response;
                }
                // Get available drivers based on pickup coordinates
                var availableDrivers = await GetAvailableDriversAsync(startLat, startLng);
                // Check if there are available drivers
                if (!availableDrivers.Any())
                {
                    return new ApiResponse<List<Driver>>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "No available drivers found within 2km radius."
                    };
                }            
                response.StatusCode = HttpStatusCode.OK;
                response.Message = $"{availableDrivers.Count} drivers found within 2km radius.";
                response.Data = availableDrivers;
            }

            catch (DbException dbEx)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Database error occurred.";
                response.ErrorMessage = dbEx.Message;
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred.";
                response.ErrorMessage = ex.Message;
            }
            return response;
        }
        private TripDTO ConvertToDto(Trip trip)
        {
            return new TripDTO
            {
                Id = trip.Id,
                CustomerId = trip.CustomerId,
                DriverId = trip.DriverId,
                SourceLatitude = trip.SourceLatitude,
                SourceLongitude = trip.SourceLongitude,
                DestinationLatitude = trip.DestinationLatitude,
                DestinationLongitude = trip.DestinationLongitude,
                Status = (TripStatus)Enum.Parse(typeof(TripStatus), trip.Status.ToString())
            };
        }
        private Trip ConvertToEntity(TripDTO tripDto)
        {
            return new Trip
            {
                Id = tripDto.Id,
                CustomerId = tripDto.CustomerId,
                DriverId = tripDto.DriverId,
                SourceLatitude = tripDto.SourceLatitude,
                SourceLongitude = tripDto.SourceLongitude,
                DestinationLatitude = tripDto.DestinationLatitude,
                DestinationLongitude = tripDto.DestinationLongitude,
                Status = tripDto.Status.ToString()[0].ToString()
            };
        }
        public async Task<ApiResponse<Trip>> ConfirmPickupAsync(
            long customerId, 
            long driverId, 
            double startLat, 
            double startLng, 
            double endLat, 
            double endLng)
        {
            var response = new ApiResponse<Trip>();
            try
            {
                // Check if Customer exists
                var customerExists = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                if (customerExists == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Customer not found.";
                    return response;
                }

                // Check if Driver exists
                var driverExists = await _unitOfWork.DriverRepository.GetByIdAsync(driverId);
                if (driverExists == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "Driver not found.";
                    return response;
                }
                byte[] randomCreateAt = new byte[8]; 
                new Random().NextBytes(randomCreateAt);

                // Create a new trip
                var newTripDto = new TripDTO
                {
                    CustomerId = customerId,
                    DriverId = driverId, // Assign the driver that confirmed the trip
                    SourceLatitude = startLat,
                    SourceLongitude = startLng,                  
                    DestinationLatitude= endLat,
                    DestinationLongitude = endLng,
                    Status = TripStatus.O
                };

                // Convert DTO to entity
                var newTrip = ConvertToEntity(newTripDto);
                // Get the next trip ID
                var maxId = await _unitOfWork.TripRepository.GetMaxTripIdAsync();
                newTrip.Id = maxId + 1; // Assign the new ID
                // Save changes to the database
                await _unitOfWork.TripRepository.CreateTrip(newTrip);
                 await _unitOfWork.SaveChangeAsync();

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Trip status updated to OnTrip.";
                response.Data = newTrip;
            }
            catch (DbException dbEx)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Database error occurred.";
                response.ErrorMessages = new List<string> { dbEx.Message, dbEx.StackTrace };
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred.";
                response.ErrorMessages = new List<string> { ex.Message, ex.StackTrace };
            }
            return response;
        }

        public async Task<ApiResponse<Trip>> CompleteTripAsync(long tripId, long driverId)
        {
            var response = new ApiResponse<Trip>();
                try
                {
                    // Retrieve the trip by ID
                    var trip = await _unitOfWork.TripRepository.GetByIdAsync(tripId);
                    if (trip == null)
                    {
                        return new ApiResponse<Trip>
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Message = "Trip not found."
                        };
                    }
                // Check if the driverId matches the DriverId of the trip
                if (trip.DriverId != driverId) // Assuming DriverId is a property of the Trip entity
                {
                    return new ApiResponse<Trip>
                    {
                        StatusCode = HttpStatusCode.Forbidden,
                        Message = "You are not authorized to complete this trip."
                    };
                }
                // Update the trip status to "Completed"
                trip.Status = TripStatus.C.ToString();
                    // Save changes to the database
                    await _unitOfWork.TripRepository.UpdateTrip(trip);
                    await _unitOfWork.SaveChangeAsync();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "Trip status updated to Completed.";
                    response.Data = trip;
                }
                catch (DbException dbEx)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Message = "Database error occurred.";
                    response.ErrorMessage = dbEx.Message;
                }
                catch (Exception ex)
                {
                     response.StatusCode = HttpStatusCode.InternalServerError;
                     response.Message = "An unexpected error occurred.";
                     response.ErrorMessage = ex.Message;
                }
            return response;
        }

    }
}
