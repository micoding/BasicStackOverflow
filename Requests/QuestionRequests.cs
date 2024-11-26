using BasicStackOverflow.Authorization;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Exceptions;
using BasicStackOverflow.Models;
using BasicStackOverflow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicStackOverflow.Requests;

public class QuestionRequests
{
    public static async Task<IResult> Create([FromBody] CreateQuestionDTO questionDto, IPostsService postsService)
    {
        return Results.Ok(await postsService.CreateQuestion(questionDto));
    }

    public static async Task<IResult> GetAll([AsParameters] PagesQuery query, IPostsService service)
    {
        var result = await service.GetAllQuestions(query);

        return result.Items.Count <= 0 ? Results.NotFound() : Results.Ok(result);
    }

    public static async Task<IResult> GetById([FromRoute] int id, IPostsService service)
    {
        var result = await service.GetQuestionDTO(id);

        return result == null ? Results.NotFound() : Results.Ok(result);
    }

    public static async Task<IResult> Delete([FromRoute] int id, IPostsService service, BasicStackOverflowContext db,
        IAuthorizationService authService, IUserContextService userContext)
    {
        var question = await service.GetQuestion(id);

        var authorizationResult =
            authService.AuthorizeAsync(userContext.User, question, new PostOperationRequirement()).Result;
        if (!authorizationResult.Succeeded) throw new ForbidException();

        db.Questions.Remove(question);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    public static async Task<IResult> MarkAnswered([FromRoute] int id, [FromRoute] int answerId,
        IPostsService service, IAuthorizationService authService, IUserContextService userContext,
        BasicStackOverflowContext db)
    {
        var question = await service.GetQuestion(id);
        var answer = question.Answers.FirstOrDefault(x => x.QuestionId == id);

        if (answer == null)
            throw new NotFoundException("Answer not found");

        var authorizationResult =
            authService.AuthorizeAsync(userContext.User, question, new PostOperationRequirement()).Result;
        if (!authorizationResult.Succeeded) throw new ForbidException();

        if (question.Answered)
            throw new QuestionResolvedException("This question has already been answered.");

        answer.BestAnswer = true;
        question.Answered = true;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    public static async Task<IResult> AddAnswer([FromRoute] int id, [FromBody] CreateAnswerDTO createAnswerDto,
        IPostsService postsService)
    {
        var newAnswerId = await postsService.CreateAnswer(id, createAnswerDto);
        return Results.Created($"question/{id}", null);
    }
}