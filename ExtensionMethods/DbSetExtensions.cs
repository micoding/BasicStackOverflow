using BasicStackOverflow.Entities;
using Microsoft.EntityFrameworkCore;

namespace BasicStackOverflow.ExtensionMethods;

public static class DbSetExtensions
{
    public static IQueryable<Question> GetQuestionDTOs(this DbSet<Question> questions)
    {
        return questions.AsNoTracking()
            .Include(x => x.Answers).ThenInclude(x => x.Author)
            .Include(x => x.Answers).ThenInclude(x => x.Comments).ThenInclude(x => x.Author)
            .Include(x => x.Tags)
            .Include(x => x.Comments).ThenInclude(x => x.Author)
            .Include(x => x.Author);
    }

    public static IQueryable<User> GetUserDTOs(this DbSet<User> users)
    {
        return users.AsNoTracking()
            .Include(x => x.Posts)
            .Include(x => x.Comments)
            .Include(x => x.Votes);
    }
}