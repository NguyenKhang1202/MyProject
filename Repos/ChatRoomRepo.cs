using MyProject.Context;
using MyProject.Domain;
using MyProject.Infrastructures;

namespace MyProject.Repos;

public interface IChatRoomRepo: IRepository<ChatRoom>
{
}

public class ChatRoomRepo(MyDbContext context) : BaseRepository<ChatRoom>(context), IChatRoomRepo
{
}