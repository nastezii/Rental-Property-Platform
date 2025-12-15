using Application.BaseService;
using Domain.Entities;

namespace Application.ListingService;

public interface IListingService : IBaseService<Listing>
{
    Task<Listing> CreateNewUserListingAsync(User user, Property property, List<PropertyAmenity> amenities, Listing listing);
    Task<List<Listing>> GetActiveListingAsync(int page, int pageSize);

    Task<Listing> CreateNewUserListingWithExceptionAsync
        (User user, Property property, List<PropertyAmenity> amenities, Listing listing);
}