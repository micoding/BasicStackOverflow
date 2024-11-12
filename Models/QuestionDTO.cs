namespace BasicStackOverflow.Models;

public class QuestionDTO
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreationDate { get; set; }
    public string Username { get; set; }
    public List<TagDTO> Tags { get; set; } = new();
    public List<AnswerDTO> Answers { get; set; } = new();
    public List<CommentDTO> Comments { get; set; } = new();
}