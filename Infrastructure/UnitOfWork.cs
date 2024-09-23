using Infrastructure.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class UnitOfWork
    {
        private readonly UberSystemContext _dbContext;
        private readonly UserRepo _userRepository;
        public UnitOfWork(UberSystemContext dbContext, UserRepo userRepo)
        {
            _dbContext = dbContext;
            _userRepository = userRepo;
        }

        public UserRepo UserRepository => _userRepository;

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
