using Application.BaseService;
using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Application.PropertyAmenityService;

public class PropertyAmenityService(IBaseRepository<PropertyAmenity> baseRepository) 
    : BaseService<PropertyAmenity>(baseRepository), IPropertyAmenityService;