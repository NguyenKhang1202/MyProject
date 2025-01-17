using MyProject.Context;
using MyProject.Domain;
using MyProject.Infrastructures;

namespace MyProject.Repos;

public interface IVerificationCodeRepo: IRepository<VerificationCode>
{
}

public class VerificationCodeRepo(MyDbContext context) : BaseRepository<VerificationCode>(context), IVerificationCodeRepo
{
}