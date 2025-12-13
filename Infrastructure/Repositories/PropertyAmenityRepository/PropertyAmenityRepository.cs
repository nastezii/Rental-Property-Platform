using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.PropertyAmenityRepository;

public class PropertyAmenityRepository(ApplicationDbContext context)
    : BaseRepository<PropertyAmenity>(context), IPropertyAmenityRepository;