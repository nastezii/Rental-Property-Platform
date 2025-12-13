using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.ListingRepository;

public class ListingRepository(ApplicationDbContext context) : BaseRepository<Listing>(context), IListingRepository;