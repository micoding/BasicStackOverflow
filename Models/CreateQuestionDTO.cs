namespace BasicStackOverflow.Models;

public class CreateQuestionDTO
{
    public string Title { get; set; }
    public string Content { get; set; }
    public List<TagDTO> Tags { get; set; } = new();
}