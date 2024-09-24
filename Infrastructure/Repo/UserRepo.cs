using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo
{
    public class UserRepo : GenericRepo<User>
    {
        private readonly UberSystemContext _dbContext;

        public UserRepo(UberSystemContext context) : base(context)
        {
            _dbContext = context;
        }
        public async Task<User> GetByIdAsync(long id)
            => await _context.Users.FindAsync(id);
        
        public async Task<int> GetCountAsync()
            => await _context.Users.CountAsync();
        
        public async Task<User> GetUserByEmailAddressAndPassword(string email, string password)
            => await _dbContext.Users
                .FirstOrDefaultAsync(record => record.Email == email && record.Password == password);                
        public async Task<bool> CheckEmailAddressExisted(string email) 
            => await _dbContext.Users.AnyAsync(u => u.Email == email);
        public async Task<User> GetUserByConfirmationToken(string token) 
            => await _dbContext.Users.SingleOrDefaultAsync(
                u => u.Token == token);
    }
}
