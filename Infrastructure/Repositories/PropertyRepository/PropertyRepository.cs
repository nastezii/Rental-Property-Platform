using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.PropertyRepository;

public class PropertyRepository(ApplicationDbContext context) : BaseRepository<Property>(context), IPropertyRepository;