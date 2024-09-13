namespace FetchInfo;

public class User
{
    public User(string id, string name)
    {
        if(Guid.TryParse(id, out var guid))
        {
            Id = guid;
        }
        else
        {
            throw new InvalidCastException($"Can't cast {id} to Guid");
        }
        
        Name = name;
    }
    
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}