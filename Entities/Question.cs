namespace BasicStackOverflow.Entities;

public class Question : Post
{
    public string Title { get; set; }
    public List<Tag> Tags { get; set; } = new();
    public List<Answer> Answers { get; set; } = new();
    
    public bool Answered { get; set; } = false;
}