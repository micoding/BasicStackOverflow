namespace BasicStackOverflow.Models;

public class CreateAnswerDTO
{
    public string Content { get; set; }
    public int AuthorId { get; set; }
    public int QuestionId { get; set; }
}