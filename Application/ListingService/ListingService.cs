using Application.BaseService;
using Application.PropertyAmenityService;
using Application.PropertyService;
using Application.UserService;
using Domain;
using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace Application.ListingService;

public class ListingService(IBaseRepository<Listing> baseRepository, IUserService userService, IPropertyService propertyService,
    IPropertyAmenityService propertyAmenityService) : BaseService<Listing>(baseRepository), IListingService
{
    public async Task<Listing> CreateNewUserListingAsync(User user, Property property, List<PropertyAmenity> amenities, Listing listing)
    {
        await BeginTransactionAsync();
        
        try
        {
            var addedUser = await userService.AddAsync(user);
            property.UserId = addedUser.Id;
            
            var addedProperty = await propertyService.AddAsync(property);
            listing.PropertyId = addedProperty.Id;

            if (amenities.Any())
            {
                amenities.ForEach(x => x.PropertyId = addedProperty.Id);
                await propertyAmenityService.AddRangeAsync(amenities);
            }
            
            var addedListing = await AddAsync(listing);
            await CommitTransactionAsync();
            
            return GetAll()
                .Include(x => x.Property)
                    .ThenInclude(x => x.PropertyAmenities)
                        .ThenInclude(x => x.Amenity)
                .Include(x => x.Property)
                    .ThenInclude(x => x.User)
                .First(x => x.Id == addedListing.Id);
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync(ex);
        }
        
        return null;
    }

    public async Task<List<Listing>> GetActiveListingAsync(int page, int pageSize)
    {
        return await GetAll()
            .Where(x => x.Status == ListingStatus.Active)
            .Include(x => x.Property)
                .ThenInclude(x => x.User)
            .Include(x => x.Property)
                .ThenInclude(x => x.PropertyAmenities)
                    .ThenInclude(x => x.Amenity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<Listing> CreateNewUserListingWithExceptionAsync
        (User user, Property property, List<PropertyAmenity> amenities, Listing listing)
    {
        var response = new Listing();
        await BeginTransactionAsync();
        
        try
        {
            var addedUser = await userService.AddAsync(user);
            property.UserId = addedUser.Id;
            
            var addedProperty = await propertyService.AddAsync(property);
            listing.PropertyId = addedProperty.Id;

            if (amenities.Any())
            {
                amenities.ForEach(x => x.PropertyId = addedProperty.Id);
                await propertyAmenityService.AddRangeAsync(amenities);
            }
            
            response = await AddAsync(listing);
            throw new Exception("Error while adding new listing");
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync(ex);
        }
        
        return response;
    }
}