namespace FetchInfo;

public class User
{
    public User(string id, string name, string photoPath)
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
        PhotoPath = photoPath;
    }
    
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
}