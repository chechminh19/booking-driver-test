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
        public async Task<User> GetUserByEmailAddressAndPassword(string email, string password)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(record => record.Email == email && record.Password == password);
           

            return user;
        }
    }
}
