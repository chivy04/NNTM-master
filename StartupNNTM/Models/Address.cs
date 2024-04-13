using StartupNNTM.Models;

public class Address
{   
    public Guid Id { get; set; }
    public string Name { get; set; }

    public ICollection<Post> Post { get; set; }

}

