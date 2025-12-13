using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Infrastructure.Repositories.UserRepository;

public interface IUserRepository : IBaseRepository<User>;