using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.ReviewRepository;

public class ReviewRepository(ApplicationDbContext context) : BaseRepository<Review>(context), IReviewRepository;