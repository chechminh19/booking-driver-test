using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class CustomerRepo : GenericRepo<Customer>
    {
        private readonly UberSystemContext _dbContext;

        public CustomerRepo(UberSystemContext context) : base(context)
        {
            _dbContext = context;
        }
        public async Task<Customer> GetByIdAsync(long id)
          => await _context.Customers.FindAsync(id);
        public async Task<int> GetCountAsync()
           => await _context.Customers.CountAsync();
        public async Task<Customer> GetIdUserFromCustomer(long userId)
           => await _context.Customers.FirstOrDefaultAsync(d => d.UserId == userId);
    }
}
