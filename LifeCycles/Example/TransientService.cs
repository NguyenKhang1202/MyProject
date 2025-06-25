namespace MyProject.LifeCycles.Example;

public class TransientService : IGuidService
{
    private readonly Guid _guid = Guid.NewGuid();
    public Guid GetGuid() => _guid;
}
