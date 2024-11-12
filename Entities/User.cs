namespace BasicStackOverflow.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public List<Post> Posts { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public List<Vote> Votes { get; set; } = new();
}