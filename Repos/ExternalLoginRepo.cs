using MyProject.Context;
using MyProject.Domain;
using MyProject.Infrastructures;

namespace MyProject.Repos;

public interface IExternalLoginRepo: IRepository<ExternalLogin>
{
}

public class ExternalLoginRepo(MyDbContext context) : BaseRepository<ExternalLogin>(context), IExternalLoginRepo
{
}