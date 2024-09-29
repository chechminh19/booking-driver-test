using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class DriverRepo : GenericRepo<Driver>
    {
        private readonly UberSystemContext _dbContext;

        public DriverRepo(UberSystemContext context) : base(context)
        {
            _dbContext = context;
        }
        public async Task<Driver> GetByIdAsync(long id)
           => await _context.Drivers.FindAsync(id);
        public async Task<long> GetMaxIdAsync()
        {
            return await _context.Drivers.MaxAsync(u => u.Id);
        }
        public async Task<Driver> GetIdUserFromDriver(long userId)     
           => await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
        public async Task<List<Driver>> GetAvailableDrivers()
        {
            var availableDrivers = await _context.Drivers
       .Include(driver => driver.User)
       .Include(driver => driver.Ratings)
       .Include(driver => driver.Trips) 
       .Where(driver => driver.Trips.All(trip => trip.Status != "O"))
       .ToListAsync();

            // Sắp xếp tài xế theo điểm đánh giá trung bình giảm dần
            return availableDrivers
                .OrderByDescending(driver => driver.Ratings.Any() ? driver.Ratings.Average(r => r.Rating1 ?? 0) : 0) // Trả về 0 nếu không có đánh giá
                .ToList();
        }
    }
}
