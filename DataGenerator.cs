using BasicStackOverflow.Entities;
using Bogus;

namespace BasicStackOverflow;

public class DataGenerator
{
    public static void Seed(BasicStackOverflowContext dbContext)
    {
        var userGenerator = new Faker<User>().RuleFor(x => x.Username, f => f.Person.UserName);
        var tagGenerator = new Faker<Tag>().RuleFor(x => x.Name, f => f.Random.AlphaNumeric(10));

        var users = userGenerator.Generate(100);

        dbContext.AddRange(users);
        //dbContext.SaveChanges();
    }
}