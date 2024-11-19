using System.Text;
using System.Text.Json.Serialization;
using AutoMapper;
using BasicStackOverflow;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Exceptions;
using BasicStackOverflow.Middleware;
using BasicStackOverflow.Models;
using BasicStackOverflow.Models.Validators;
using BasicStackOverflow.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContext<BasicStackOverflowContext>(
    option => option.UseMySql(builder.Configuration.GetConnectionString("BasicStackOverflowConnectionString"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("BasicStackOverflowConnectionString")))
);

builder.Services.AddAutoMapper(typeof(BasicStackOverflowMappingProfile));

builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<RequestTimeMiddleware>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IValidator<CreateUserDTO>, CreateUserDTOValidator>();

var authenticationSettings = new AuthenticationSettings();
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);
builder.Services.AddSingleton(authenticationSettings);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = authenticationSettings.JwtIssuer,
        ValidAudience = authenticationSettings.JwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey))
    };
});

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
builder.Logging.ClearProviders();
builder.Host.UseNLog();


var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestTimeMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetService<BasicStackOverflowContext>();

DataGenerator.Seed(dbContext);
//
// app.MapGet("getAllUsers", async (BasicStackOverflowContext db, IMapper mapper) =>
//     {
//         var users = await db.Users.AsNoTracking().Include(x => x.Posts).Include(x => x.Comments)
//             .Include(x => x.Votes).ToListAsync();
//         
//         if(!users.Any())
//             throw new NotFoundException("Users not found");
//         
//         var usersDto = mapper.Map<List<User>>(users);
//         return usersDto;
//         
//     });
//
// app.MapPost("getAllUsers", async ([FromQuery]string nameContains, [FromQuery] int? minPostsAmount,  BasicStackOverflowContext db) =>
// {
//     minPostsAmount ??= 0;
//     
//     var usersFiltered = await db.Users.AsNoTracking().Where(x => x.Username.Contains(nameContains) && x.Posts.Count >= minPostsAmount)
//         .Select(x => x.Username).ToListAsync();
//     
//     return usersFiltered.Any() ? Results.Ok(usersFiltered) : Results.NotFound();
// });
//
// app.MapGet("getUserQuestions/{id}", async ([FromRoute]int id, BasicStackOverflowContext db) =>
// {
//     var userQuestions = await db.Questions.AsNoTracking().Where(x => x.AuthorId == id).Include(x => x.Comments)
//         .Include(x => x.Answers).Include(x => x.Tags)
//         .Select(x => new
//         {
//             x.Title,
//             Question = x.Content,
//             Comments = x.Comments.Select(y => new { y.Content, y.CreationDate }),
//             Answers = x.Answers.Select(y => new
//             {
//                 y.Content, y.CreationDate,
//                 Comments = y.Comments.Select(z => new { y.Content, y.Author, y.CreationDate })
//             }),
//             Tags = x.Tags.Select(y => new { y.Name })
//         }).ToListAsync();
//
//     if (!userQuestions.Any())
//         throw new NotFoundException($"No user found with id: {id}");
//
//     return userQuestions;
// });
//
// app.MapGet("getUserAnswers", async ([FromQuery]int id, BasicStackOverflowContext db) =>
// {
//    var answers = await db.Answers.AsNoTracking()
//        .Include(x => x.Question) //.Include(x => x.Question)
//         .Where(x => x.Id == id).ToListAsync();
// });

app.MapGet("question/{id:int}", async ([FromRoute] int id, IPostsService service) =>
{
    var result = await service.GetQuestion(id);
    return Results.Ok(result);
});

app.MapGet("questions", async (IPostsService service) =>
{
    var result = await service.GetAllQuestions();
    return Results.Ok(result);
});

app.MapGet("users", async (IUsersService service) =>
{
    var result = await service.GetAllUsers();
    return Results.Ok(result);
});

app.MapGet("user/{id:int}", async ([FromRoute] int id, IUsersService service) =>
{
    var result = await service.FindUserByIdAsync(id);
    return Results.Ok(result);
});

app.MapPost("user", async ([FromBody] CreateUserDTO userDto, IMapper mapper, BasicStackOverflowContext db) =>
{
    var newUser = mapper.Map<User>(userDto);
    await db.AddAsync(newUser);
    await db.SaveChangesAsync();
    return Results.Ok(newUser);
});

app.MapDelete("user/{id:int}", async ([FromRoute] int id, BasicStackOverflowContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);

    if (user == null)
        throw new NotFoundException("Users not found");

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapPost("questions",
    async ([FromBody] CreateQuestionDTO questionDto, IPostsService postsService) =>
    {
        return Results.Ok(await postsService.CreateQuestion(questionDto));
    });

app.MapPost("question/{id:int}/answer",
    async ([FromRoute] int id, [FromBody] CreateAnswerDTO createAnswerDto, IPostsService postsService) =>
    {
        var newAnswerId = await postsService.CreateAnswer(id, createAnswerDto);
        return Results.Created($"question/{id}", null);
    });

app.MapPost("users/register",
    async ([FromBody] CreateUserDTO userDto, IUsersService usersService, IValidator<CreateUserDTO> validator) =>
    {
        var validationResult = await validator.ValidateAsync(userDto);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var user = await usersService.RegisterUser(userDto);

        return Results.Created($"users/{user.Id}", null);
    });

app.MapPost("users/login",
    async ([FromBody] LoginUserDTO userDto, IUsersService usersService) =>
    {
        return Results.Ok(usersService.GenerateJwtToken(userDto));
    });


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();