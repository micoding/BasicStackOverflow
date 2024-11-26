using System.Linq.Expressions;
using AutoMapper;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Exceptions;
using BasicStackOverflow.ExtensionMethods;
using BasicStackOverflow.Models;
using Microsoft.EntityFrameworkCore;

namespace BasicStackOverflow.Services;

public interface IPostsService
{
    Task<QuestionDTO> GetQuestionDTO(int id);
    Task<Question> GetQuestion(int id);
    Task<PagedResult<QuestionDTO>> GetAllQuestions(PagesQuery query);
    Task<int> CreateQuestion(CreateQuestionDTO questionDto);
    Task<int> CreateAnswer(int questionId, CreateAnswerDTO answerDto);
    Task<IEnumerable<Answer>> GetAnswers(int questionId);
}

public class PostsService : IPostsService
{
    private readonly BasicStackOverflowContext _context;
    private readonly ILogger<PostsService> _logger;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContext;

    public PostsService(BasicStackOverflowContext dbContext, IMapper mapper, ILogger<PostsService> logger,
        IUserContextService userContext)
    {
        _context = dbContext;
        _mapper = mapper;
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<Question> GetQuestion(int id)
    {
        _logger.LogInformation($"Question with ID {id} GET called.");

        var question = await _context.Questions.FirstOrDefaultAsync(x => x.Id == id);

        if (question is null)
            throw new NotFoundException($"Question with ID {id} not found.");

        return question;
    }

    public async Task<IEnumerable<Answer>> GetAnswers(int questionId)
    {
        var answers = await _context.Answers.Where(a => a.QuestionId == questionId).ToListAsync();

        if (answers is null)
            throw new NotFoundException($"Answers for question with ID {questionId} not found.");

        return answers;
    }

    public async Task<QuestionDTO> GetQuestionDTO(int id)
    {
        var question = await _context.Questions.IncludeForQuestionDTOs().FirstOrDefaultAsync(x => x.Id == id);

        if (question is null)
            throw new NotFoundException($"Question with ID {id} not found.");

        var result = _mapper.Map<QuestionDTO>(question);

        return result;
    }

    public async Task<PagedResult<QuestionDTO>> GetAllQuestions(PagesQuery query)
    {
        _logger.LogInformation("Getting all Questions from DB.");

        var baseQuery = _context.Questions.IncludeForQuestionDTOs()
            .Where(x => query.SearchString == null ||
                        (x.Title.Contains(query.SearchString)
                         || x.Content.Contains(query.SearchString)));

        if (!string.IsNullOrEmpty(query.SortBy))
        {
            var columnSelector = new Dictionary<string, Expression<Func<Question, object>>>()
            {
                { nameof(Question.Title), x => x.Title },
                { nameof(Question.Content), x => x.Content },
                { nameof(Question.CreationDate), x => x.CreationDate },
                { nameof(Question.AuthorId), x => x.AuthorId}
            };
                baseQuery = (bool)query.SortAscending ? baseQuery.OrderBy(columnSelector[query.SortBy]) : baseQuery.OrderByDescending(columnSelector[query.SortBy]);
        }

        var questions = await baseQuery.Skip(query.SkipForPage ?? 0).Take(query.PageSize ?? 1).ToListAsync();
        
        var totalResults = await baseQuery.CountAsync();
        
        if (!questions.Any())
            throw new NotFoundException("No Questions found.");
        
        var result = _mapper.Map<List<QuestionDTO>>(questions);
        
        var pagedResult = new PagedResult<QuestionDTO>(result, totalResults, query.PageSize ?? 1, query.PageNumber ?? 1);

        return pagedResult;
    }

    public async Task<int> CreateQuestion(CreateQuestionDTO questionDto)
    {
        var question = _mapper.Map<Question>(questionDto);

        question.AuthorId = _userContext.GetUserId.Value;
        if (question.Tags.Any())
        {
            var allTags = await _context.Tags.ToListAsync();
            var preparedTags = question.Tags.Select(tag => allTags.FirstOrDefault(x => x.Name == tag.Name))
                .Where(x => x != null).ToList();
            preparedTags.ForEach(t => question.Tags.RemoveAll(tag => tag.Name == t.Name));

            question.Tags.AddRange(preparedTags);
        }

        await _context.AddAsync(question);
        await _context.SaveChangesAsync();
        return question.Id;
    }

    public async Task<int> CreateAnswer(int questionId, CreateAnswerDTO answerDto)
    {
        var question = await _context.Questions.FirstOrDefaultAsync(x => x.Id == questionId);

        if (question is null)
            throw new NotFoundException($"Question with ID {questionId} not found.");

        answerDto.QuestionId = question.Id;

        var answer = _mapper.Map<Answer>(answerDto);

        answer.AuthorId = _userContext.GetUserId.Value;

        await _context.Answers.AddAsync(answer);
        await _context.SaveChangesAsync();

        return answer.Id;
    }
}