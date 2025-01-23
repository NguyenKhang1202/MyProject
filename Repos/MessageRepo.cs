using MyProject.Context;
using MyProject.Domain;
using MyProject.Infrastructures;

namespace MyProject.Repos;

public interface IMessageRepo: IRepository<Message>
{
}

public class MessageRepo(MyDbContext context) : BaseRepository<Message>(context), IMessageRepo
{
}