namespace BasicStackOverflow.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreationDate { get; set; }
    public User Author { get; set; }
    public int AuthorId { get; set; }
    public Post Post { get; set; }
    public int PostId { get; set; }
}