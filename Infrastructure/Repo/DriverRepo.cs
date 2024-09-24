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
        public async Task<int> GetCountAsync()
           => await _context.Drivers.CountAsync();
        public async Task<Driver> GetIdUserFromDriver(long userId)     
           => await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
        
    }
}
