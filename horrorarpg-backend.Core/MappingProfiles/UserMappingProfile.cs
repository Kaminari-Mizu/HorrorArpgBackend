using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using horrorarpg_backend.Core.DTOs;
using horrorarpg_backend.Core.Entities;

namespace horrorarpg_backend.Core.MappingProfiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile() 
        {
            CreateMap<UserEntity, UserDto>();

            // CHANGE: Ignore new UserSave nav prop in User-to-DTO maps (avoids circular refs)
            //CreateMap<UserEntity, UserDto>()
            //    .ForMember(dest => dest.UserSave, opt => opt.Ignore());  // If UserDto adds it later; else unnecessary
            CreateMap<LoginRequestDto, UserEntity>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
