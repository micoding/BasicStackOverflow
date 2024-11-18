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

        var question = await _context.Questions.GetQuestionDTOs().FirstOrDefaultAsync(x => x.Id == id);

        if (question == null)
            throw new NotFoundException($"Question with ID {id} not found.");

        var result = _mapper.Map<QuestionDTO>(question);

        return result;
    }

    public async Task<List<QuestionDTO>> GetAllQuestions()
    {
        _logger.LogInformation("Getting all Questions from DB.");

        var questions = await _context.Questions.GetQuestionDTOs().ToListAsync();

        if (!questions.Any())
            throw new NotFoundException("No Questions found.");

        return _mapper.Map<List<QuestionDTO>>(questions);
    }
}