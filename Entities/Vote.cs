namespace BasicStackOverflow.Entities;

public class Vote
{
    public bool Value { get; set; }
    public User UserVoted { get; set; }
    public int UserId { get; set; }
    public Answer Answer { get; set; }
    public int AnswerId { get; set; }
}