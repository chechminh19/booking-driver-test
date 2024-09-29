using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
   
    public class RatingRepo : GenericRepo<Rating>
    { 
        private readonly UberSystemContext _dbContext;
        public RatingRepo(UberSystemContext context) : base(context)
        {
            _dbContext = context;
        }
        public async Task<Rating> GetRatingByTripIdAsync(long tripId)
        {
            return await _context.Ratings.FirstOrDefaultAsync(r => r.TripId == tripId);
        }
        public async Task<long> GetMaxRatingIdAsync()
        {
            return await _context.Ratings.MaxAsync(r => r.Id);
        }
    }
}
