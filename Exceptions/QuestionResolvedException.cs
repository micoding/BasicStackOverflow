namespace BasicStackOverflow.Exceptions;

public class QuestionResolvedException : Exception
{
    public QuestionResolvedException(string message) : base(message)
    {
    }
}