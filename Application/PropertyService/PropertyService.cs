using Application.BaseService;
using Application.PropertyAmenityService;
using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace Application.PropertyService;

public class PropertyService(IBaseRepository<Property> baseRepository, IPropertyAmenityService propertyAmenityService)
    : BaseService<Property>(baseRepository), IPropertyService
{
    public async Task<List<Property>> GetTopRatePropertiesAsync()
    {
        return await GetAll()
            .Include(x => x.Reviews)
            .Include(x => x.PropertyAmenities)
            .ThenInclude(x => x.Amenity)
            .OrderByDescending(x => x.Reviews.DefaultIfEmpty().Average(r => r == null ? 0 : r.Rating))
            .ToListAsync();
    }

    public async Task<Property> UpdatePropertyAsync(int id, Property property, List<PropertyAmenity> amenities)
    {
        await BeginTransactionAsync();

        try
        {
            var existingItem = await GetAll()
                .Include(x => x.PropertyAmenities)
                .ThenInclude(x => x.Amenity)
                .FirstOrDefaultAsync(x => x.Id == id) ?? throw new Exception("Property not found");

            await propertyAmenityService.DeleteRangeAsync(existingItem.PropertyAmenities);
            amenities.ForEach(x => x.PropertyId = existingItem.Id);
            await propertyAmenityService.AddRangeAsync(amenities);
            
            existingItem.Title = property.Title;
            existingItem.Description = property.Description;
            existingItem.Location = property.Location;
            existingItem.PropertyType = property.PropertyType;
            
            await UpdateAsync(existingItem);
            await CommitTransactionAsync();
            
            var response = await GetAll()
                .Include(x => x.Reviews)
                .Include(x => x.PropertyAmenities)
                .ThenInclude(x => x.Amenity)
                .FirstAsync(x => x.Id == id);
            
            return response;
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync(ex);
        }
        
        return null;
    }
    
    public async Task<Property> UpdatePropertyWithExceptionAsync(int id, Property property, List<PropertyAmenity> amenities)
    {
        await BeginTransactionAsync();
        var response = new Property();

        try
        {
            var existingItem = await GetAll()
                .Include(x => x.PropertyAmenities)
                .ThenInclude(x => x.Amenity)
                .FirstOrDefaultAsync(x => x.Id == id) ?? throw new Exception("Property not found");

            await propertyAmenityService.DeleteRangeAsync(existingItem.PropertyAmenities);
            amenities.ForEach(x => x.PropertyId = existingItem.Id);
            await propertyAmenityService.AddRangeAsync(amenities);
            
            existingItem.Title = property.Title;
            existingItem.Description = property.Description;
            existingItem.Location = property.Location;
            existingItem.PropertyType = property.PropertyType;
            
            response = await UpdateAsync(existingItem);
            throw new Exception("Error while updating property");
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync(ex);
        }
        
        return response;
    }
}