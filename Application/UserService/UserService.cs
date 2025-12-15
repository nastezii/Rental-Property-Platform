using Application.BaseService;
using Application.PropertyService;
using Domain.Entities;
using Infrastructure.Repositories.BaseRepository;

namespace Application.UserService;

public class UserService(IBaseRepository<User> baseRepository, IPropertyService propertyService) 
    : BaseService<User>(baseRepository), IUserService;