namespace BasicStackOverflow.Entities;

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreationDate { get; set; }
    public User Author { get; set; }
    public int AuthorId { get; set; }
    public List<Comment> Comments { get; set; } = new();
}