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
            services.AddScoped<UnitOfWork>();
            services.AddScoped<UserService>();
            return services;
        }
    }
}
