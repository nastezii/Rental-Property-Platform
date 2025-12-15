using AutoMapper;
using Domain.Dtos.CreateNewUserListingDto;
using Domain.Dtos.General;
using Domain.Entities;

namespace WebApplication1;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<UserRequestDto, User>();
        CreateMap<User, UserResponseDto>();
        
        CreateMap<PropertyRequestDto, Property>();
        CreateMap<ListingRequestDto, Listing>();

        CreateMap<Property, PropertyResponseDto>()
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.PropertyAmenities.Select(pa => pa.Amenity)))
            .ForMember(dest => dest.Rating, 
                opt => opt.MapFrom(src =>
                    src.Reviews.Any()
                        ? src.Reviews.Average(r => r.Rating)
                        : 0
                ));

        CreateMap<Amenity, AmenityResponseDto>();
        CreateMap<Listing, ListingResponseDto>();
    }
}