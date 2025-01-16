using Microsoft.EntityFrameworkCore;
using MyProject.Context;
using MyProject.Domain;
using MyProject.Infrastructures;

namespace MyProject.Repos;

public interface IUserRepo: IRepository<User>
{
}

public class UserRepo(MyDbContext context) : BaseRepository<User>(context), IUserRepo
{
}