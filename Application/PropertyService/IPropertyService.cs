using Application.BaseService;
using Domain.Entities;

namespace Application.PropertyService;

public interface IPropertyService : IBaseService<Property>
{
    Task<List<Property>> GetTopRatePropertiesAsync();
    Task<Property> UpdatePropertyAsync(int id, Property property, List<PropertyAmenity> amenities);

    Task<Property> UpdatePropertyWithExceptionAsync(int id, Property property, List<PropertyAmenity> amenities);
}