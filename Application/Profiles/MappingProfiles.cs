using AutoMapper;
using Application.DTO.Api;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Profiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Domain.ApplicationModel, DTO.Admin.ApplicationListDto>().ReverseMap();
            CreateMap<ApplicationGroup, GroupDto>().ReverseMap();
            CreateMap<RoleModel, RoleDto>().ReverseMap();
            //  CreateMap<Address, AddressDto>().ReverseMap();
        }
    }
}