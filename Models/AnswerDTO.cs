namespace BasicStackOverflow.Models;

public class AnswerDTO
{
    public string Content { get; set; }
    public DateTime CreationDate { get; set; }
    public string Username { get; set; }
    public List<CommentDTO> Comments { get; set; } = new();
}