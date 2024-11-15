using AutoMapper;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Models;
using Microsoft.EntityFrameworkCore;
using ILogger = Castle.Core.Logging.ILogger;

namespace BasicStackOverflow.Services;

public interface IBSOService
{
    Task<QuestionDTO> GetQuestion(int id);
}

public class BSOService : IBSOService
{
    BasicStackOverflowContext _context;
    IMapper _mapper;
    ILogger<BSOService>  _logger;
    
    public BSOService(BasicStackOverflowContext dbContext, IMapper mapper, ILogger<BSOService> logger)
    {
        _context = dbContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<QuestionDTO> GetQuestion(int id)
    {
        _logger.LogInformation($"Question with ID {id} GET called.");
        var question = await _context.Questions.AsNoTracking()
            .Include(x => x.Answers).ThenInclude(x => x.Author)
            .Include(x => x.Answers).ThenInclude(x => x.Comments).ThenInclude(x=>x.Author)
            .Include(x => x.Tags)
            .Include(x => x.Comments).ThenInclude(x => x.Author)
            .Include(x => x.Author)
            .FirstAsync(x => x.Id == id);
    
        var result = _mapper.Map<QuestionDTO>(question);
        
        return result;
    }
}