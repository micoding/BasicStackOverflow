using BasicStackOverflow.Entities;
using BasicStackOverflow.Exceptions;
using BasicStackOverflow.Models;
using BasicStackOverflow.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BasicStackOverflow.Requests;

public class UserRequests
{
    public static async Task<IResult> Register([FromBody] CreateUserDTO userDto, IUsersService usersService,
        IValidator<CreateUserDTO> validator)
    {
        var validationResult = await validator.ValidateAsync(userDto);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var user = await usersService.RegisterUser(userDto);

        return Results.Created($"users/{user.Id}", null);
    }

    public static IResult LogIn([FromBody] LoginUserDTO userDto, IUsersService usersService)
    {
        var token = usersService.GenerateJwtToken(userDto);
        return Results.Ok(token);
    }

    public static async Task<IResult> DeletebyId([FromRoute] int id, BasicStackOverflowContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new NotFoundException("Users not found");

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return Results.Ok();
    }

    public static async Task<IResult> GetAll([FromQuery] string search, IUsersService service)
    {
        var result = await service.GetAllUsers(search);
        return Results.Ok(result);
    }

    public static async Task<IResult> GetById([FromRoute] int id, IUsersService service)
    {
        var result = await service.FindUserByIdAsync(id);
        return Results.Ok(result);
    }
}