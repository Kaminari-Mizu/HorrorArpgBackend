using AutoMapper;
using horrorarpg_backend.Core.DTOs;
using horrorarpg_backend.Core.Entities;

namespace horrorarpg_backend.Core.MappingProfiles
{
    public class UserSaveMappingProfile : Profile
    {
        public UserSaveMappingProfile()
        {
            CreateMap<UserSaveEntity, UserSaveResponseDto>()
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => new { X = src.PositionX, Y = src.PositionY, Z = src.PositionZ }));

            CreateMap<UserSaveDto, UserSaveEntity>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())  // Set from claims/service, not DTO
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.Parse(src.Timestamp)))
                .ForMember(dest => dest.PositionX, opt => opt.MapFrom(src => src.Position.X))
                .ForMember(dest => dest.PositionY, opt => opt.MapFrom(src => src.Position.Y))
                .ForMember(dest => dest.PositionZ, opt => opt.MapFrom(src => src.Position.Z))
                .ForMember(dest => dest.User, opt => opt.Ignore());  // Ignore nav; set in service if needed

            // For updates: Map DTO onto existing entity
            CreateMap<UserSaveDto, UserSaveEntity>(MemberList.Destination)
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.Parse(src.Timestamp)))
                .ForMember(dest => dest.PositionX, opt => opt.MapFrom(src => src.Position.X))
                .ForMember(dest => dest.PositionY, opt => opt.MapFrom(src => src.Position.Y))
                .ForMember(dest => dest.PositionZ, opt => opt.MapFrom(src => src.Position.Z));
        }
    }
}