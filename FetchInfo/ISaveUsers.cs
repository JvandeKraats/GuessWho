namespace FetchInfo;

public interface ISaveUsers
{
    Task Save(List<User> users);
}