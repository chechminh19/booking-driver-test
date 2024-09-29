using Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class TripRepo : GenericRepo<Trip>
    {
        private readonly UberSystemContext _dbContext;

        public TripRepo(UberSystemContext context) : base(context)
        {
            _dbContext = context;
        }
        public async Task CreateTripAsync(Trip trip)
        {
            _ = await _dbSet.AddAsync(trip);
            _ = await _context.SaveChangesAsync();
        }
        public async Task UpdateTrip(Trip trip)
        {
            _dbContext.Trips.Update(trip);
            await _dbContext.SaveChangesAsync();
        }
        public async Task CreateTrip(Trip trip)
        {
            await _dbContext.Trips.AddAsync(trip); 
            await _dbContext.SaveChangesAsync();
        }
        public async Task<long> GetMaxTripIdAsync()
        {
            // Retrieve the maximum trip Id from the Trips table
            return await _dbContext.Trips.MaxAsync(t => (long?)t.Id) ?? 0;
        }
        public async Task<Trip> GetOngoingTripByDriverIdAsync(long driverId)
        {
            return await _context.Trips
                .FirstOrDefaultAsync(t => t.DriverId == driverId && t.Status == TripStatus.O.ToString());
        }

    }
}
