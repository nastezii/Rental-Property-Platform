using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.UserRepository;

public class UserRepository(ApplicationDbContext context) : BaseRepository<User>(context), IUserRepository;