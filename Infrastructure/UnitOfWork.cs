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
        private readonly DriverRepo _driverRepository;
        private readonly CustomerRepo _cusRepository;
        public UnitOfWork(UberSystemContext dbContext, UserRepo userRepo, CustomerRepo cusRepository, DriverRepo driverRepo)
        {
            _dbContext = dbContext;
            _userRepository = userRepo;
            _cusRepository = cusRepository;
            _driverRepository = driverRepo;
        }

        public UserRepo UserRepository => _userRepository;
        public DriverRepo DriverRepository => _driverRepository;
        public CustomerRepo CustomerRepository => _cusRepository;

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
