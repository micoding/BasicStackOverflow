namespace BasicStackOverflow.Entities;

public class Answer : Post
{
    public Question Question { get; set; }
    public int QuestionId { get; set; }
    public List<Vote> Votes { get; set; } = new();
}