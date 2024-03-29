using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl,
                    opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.Name))
                .ForMember(dest => dest.LookingFors, opt => opt.MapFrom(src => src.UserLookingFors.Select(x => x.LookingFor)))
                .ForMember(dest => dest.Interests, opt => opt.MapFrom(src => src.UserInterests.Select(x => x.Interest)))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalcuateAge()))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City.Name));
            CreateMap<AppUser, MemberDtoWithoutIsVisible>()
                .ForMember(dest => dest.PhotoUrl,
                    opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.Name))
                .ForMember(dest => dest.LookingFors, opt => opt.MapFrom(src => src.UserLookingFors.Select(x => x.LookingFor)))
                .ForMember(dest => dest.Interests, opt => opt.MapFrom(src => src.UserInterests.Select(x => x.Interest)))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalcuateAge()))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City.Name))
                .ForMember(dest => dest.Distance, opt => opt.Ignore());
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<AppUser, MemberUpdateDto>();
            CreateMap<AppUser, BlockUserDto>();
            CreateMap<Photo, PhotoDto>();
            CreateMap<LookingFor, IdNameDto>();
            CreateMap<Interest, IdNameDto>();
            CreateMap<IntroductionUpdateDto, AppUser>();
            CreateMap<LocationDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<Message, MessageDto>()
                .ForMember(d => d.SenderUsername, o => o.MapFrom(s => s.SenderUsername))
                .ForMember(d => d.SenderKnownAs, o => o.MapFrom(s => s.Sender.KnownAs))
                .ForMember(d => d.SenderFullName, o => o.MapFrom(s => s.Sender.FullName))
                .ForMember(d => d.RecipientUsername, o => o.MapFrom(s => s.RecipientUsername))
                .ForMember(d => d.RecipientKnownAs, o => o.MapFrom(s => s.Recipient.KnownAs))
                .ForMember(d => d.RecipientFullName, o => o.MapFrom(s => s.Recipient.FullName))
                .ForMember(d => d.SenderPhotoUrl, o => o.MapFrom(s => s.Sender.Photos
                    .FirstOrDefault(x => x.IsMain).Url))
                .ForMember(d => d.RecipientPhotoUrl, o => o.MapFrom(s => s.Recipient.Photos
                    .FirstOrDefault(x => x.IsMain).Url));
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ?
                DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
        }
    }
}
