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
        private readonly RatingRepo _ratingRepository;
        private readonly TripRepo _tripRepository;

        public UnitOfWork(UberSystemContext dbContext, UserRepo userRepo, 
            CustomerRepo cusRepository, DriverRepo driverRepo, TripRepo tripRepository, RatingRepo ratingRepo)
        {
            _dbContext = dbContext;
            _userRepository = userRepo;
            _cusRepository = cusRepository;
            _driverRepository = driverRepo;
            _tripRepository = tripRepository;
            _ratingRepository = ratingRepo;
        }

        public UserRepo UserRepository => _userRepository;
        public DriverRepo DriverRepository => _driverRepository;
        public CustomerRepo CustomerRepository => _cusRepository;
        public RatingRepo RatingRepository => _ratingRepository;
        public TripRepo TripRepository => _tripRepository;
        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
