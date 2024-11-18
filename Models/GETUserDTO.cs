namespace BasicStackOverflow.Models;

public class GETUserDTO
{
    public int Id { get; set; }
    public string Username { get; set; }

    public int NumberOfPosts { get; set; }
    public int NumberOfComments { get; set; }
    public int NumberOfVotes { get; set; }
}