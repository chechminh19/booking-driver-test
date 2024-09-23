using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO;
using AutoMapper;
namespace Infrastructure.MapperConfigurationsProfile
{
    public class MapperConfigurationsProfile : Profile
    {
        public MapperConfigurationsProfile()
        {
            CreateMap<User, LoginUserDTO>().ReverseMap();
        }
    }
}
