using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.AmenityRepository;

public class AmenityRepository(ApplicationDbContext context) : BaseRepository<Amenity>(context), IAmenityRepository;