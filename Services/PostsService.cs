using AutoMapper;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Exceptions;
using BasicStackOverflow.ExtensionMethods;
using BasicStackOverflow.Models;
using Microsoft.EntityFrameworkCore;

namespace BasicStackOverflow.Services;

public interface IPostsService
{
    Task<QuestionDTO> GetQuestion(int id);
    Task<List<QuestionDTO>> GetAllQuestions();
    Task<int> CreateQuestion(CreateQuestionDTO questionDto);
    Task<int> CreateAnswer(int questionId, CreateAnswerDTO answerDto);
}

public class PostsService : IPostsService
{
    private readonly BasicStackOverflowContext _context;
    private readonly ILogger<PostsService> _logger;
    private readonly IMapper _mapper;

    public PostsService(BasicStackOverflowContext dbContext, IMapper mapper, ILogger<PostsService> logger)
    {
        _context = dbContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<QuestionDTO> GetQuestion(int id)
    {
        _logger.LogInformation($"Question with ID {id} GET called.");

        var question = await _context.Questions.IncludeForQuestionDTOs().FirstOrDefaultAsync(x => x.Id == id);

        if (question is null)
            throw new NotFoundException($"Question with ID {id} not found.");

        var result = _mapper.Map<QuestionDTO>(question);

        return result;
    }

    public async Task<List<QuestionDTO>> GetAllQuestions()
    {
        _logger.LogInformation("Getting all Questions from DB.");

        var questions = await _context.Questions.IncludeForQuestionDTOs().ToListAsync();

        if (!questions.Any())
            throw new NotFoundException("No Questions found.");

        return _mapper.Map<List<QuestionDTO>>(questions);
    }

    public async Task<int> CreateQuestion(CreateQuestionDTO questionDto)
    {
        var question = _mapper.Map<Question>(questionDto);

        if (question.Tags.Any())
        {
            var allTags = await _context.Tags.ToListAsync();
            var preparedTags = question.Tags.Select(tag => allTags.FirstOrDefault(x => x.Name == tag.Name)).Where(x => x != null).ToList();
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
        
        if(question is null)
            throw new NotFoundException($"Question with ID {questionId} not found.");
        
        answerDto.QuestionId = question.Id;
        
        var answer = _mapper.Map<Answer>(answerDto);
        
        await _context.Answers.AddAsync(answer);
        await _context.SaveChangesAsync();
        
        return answer.Id;
    }
}