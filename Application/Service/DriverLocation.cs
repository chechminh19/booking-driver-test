using Application.ServiceResponse;
using CsvHelper;
using Infrastructure;
using Infrastructure.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class LocationData
    {
        public string p_start { get; set; } = string.Empty;
        public string p_temp { get; set; } = string.Empty;
        public string p_end { get; set; } = string.Empty;
    }
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    public class DriverLocation : BackgroundService
    {
        private readonly Random _random;
        private readonly ILogger<DriverLocation> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DriverLocation(ILogger<DriverLocation> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _random = new Random();
        }
        public Location GetRandomLocationFromCsv()
        {
            using (var reader = new StreamReader("C:\\Users\\ADMIN\\Downloads\\new_clean_bo3.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<LocationData>().ToList();
                var randomRecord = records[_random.Next(records.Count)];
                var randomLocationString = GetRandomLocation(randomRecord);
                return ParseLocation(randomLocationString);
            }
        }
        public string GetRandomLocation(LocationData locationData)
        {
            var locations = new[] { locationData.p_start, locationData.p_temp, locationData.p_end };
            return locations[_random.Next(locations.Length)];
        }

        public static Location ParseLocation(string locationString)
        {
            // Split the string based on commas
            var split = locationString.Split(',');

            // Validate that there are exactly two parts (latitude, longitude)
            if (split.Length != 2)
                throw new Exception($"Invalid location format: {locationString}");

            // Clean up and remove any invalid characters like parentheses, spaces, etc.
            var latitudeString = split[0].Trim('(', ')', ' ');
            var longitudeString = split[1].Trim('(', ')', ' ');

            // Parse the cleaned strings as doubles using invariant culture
            return new Location
            {
                Latitude = double.Parse(latitudeString, CultureInfo.InvariantCulture),
                Longitude = double.Parse(longitudeString, CultureInfo.InvariantCulture)
            };
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Service is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
                        var activeDrivers = await unitOfWork.DriverRepository.GetAllAsync();
                        var updateTasks = new List<Task>();
                        foreach (var driver in activeDrivers)
                        {
                            updateTasks.Add(UpdateLocationAsync(driver));
                            _logger.LogInformation("Location change");

                        }

                        await Task.WhenAll(updateTasks);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating driver locations: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }

        public async Task<ServiceResponse<string>> UpdateLocationAsync(Driver driver)
        {
            var response = new ServiceResponse<string>();
            try
            {
               
                var randomLocation = GetRandomLocationFromCsv();

                var random = new Random();
                driver.LocationLatitude = randomLocation.Latitude + (random.NextDouble() - 0.5) * 0.001;
                driver.LocationLongitude = randomLocation.Longitude + (random.NextDouble() - 0.5) * 0.001;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
                    // Check for ongoing trip
                    var ongoingTrip = await unitOfWork.TripRepository.GetOngoingTripByDriverIdAsync(driver.Id);
                    if (ongoingTrip == null)
                    {
                        // No ongoing trip, driver is waiting for a customer
                        Console.WriteLine($"Driver ID: {driver.Id} is waiting for a customer.");
                    }
                    else
                    {
                        // There is an ongoing trip
                        var tripStatus = ongoingTrip.Status; // Assuming Status is a string that corresponds to the enum

                        if (tripStatus == TripStatus.O.ToString())
                        {
                            Console.WriteLine($"Driver ID: {driver.Id} dang thuc hien nhiem vu");
                        }
                        else if (tripStatus == TripStatus.C.ToString() || tripStatus == TripStatus.R.ToString())
                        {
                            Console.WriteLine($"Driver ID: {driver.Id} is dang cho khach");
                        }
                    }
                    await unitOfWork.DriverRepository.Update(driver);
                }
                Console.WriteLine(driver.LocationLatitude);
                Console.WriteLine(driver.LocationLongitude);

                response.Data = $"Driver {driver.Id} updated successfully with new location: " +
                                $"Latitude {driver.LocationLatitude}, Longitude {driver.LocationLongitude}.";
                response.Message = "Driver location updated successfully.";
                response.Success = true;
                await Task.Delay(500);
            }
            catch (DbException ex)
            {
                response.Success = false;
                response.Message = "Database error occurred.";
                response.ErrorMessages = new List<string> { ex.Message };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An unexpected error occurred.";
                response.ErrorMessages = new List<string> { ex.Message };
            }

            return response;
        }
}
}

