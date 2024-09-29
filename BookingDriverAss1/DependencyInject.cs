using Application.Service;
using Infrastructure;
using Infrastructure.Repo;

namespace BookingDriverAss1
{
    public static class DependencyInject
    {
        public static IServiceCollection AddWebAPIService(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHealthChecks();          
            
            services.AddScoped<UserRepo>();
            services.AddScoped<DriverRepo>();
            services.AddScoped<CustomerRepo>();
            services.AddScoped<RatingRepo>();
            services.AddScoped<TripRepo>();
            services.AddScoped<UnitOfWork>();       
            services.AddScoped<UserService>();
            services.AddScoped<RatingService>();
            services.AddScoped<TripService>();
            services.AddScoped<DriverLocation>();
            return services;
        }
    }
}
