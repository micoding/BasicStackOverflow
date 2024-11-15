using System.Text.Json.Serialization;
using AutoMapper;
using BasicStackOverflow;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Models;
using BasicStackOverflow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using static System.Int32;
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

builder.Services.AddScoped<IBSOService, BSOService>();

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger(); 
builder.Logging.ClearProviders();
builder.Host.UseNLog();


var app = builder.Build();

var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetService<BasicStackOverflowContext>();

DataGenerator.Seed(dbContext);

app.MapGet("getAllUsers",
    async (BasicStackOverflowContext db) => await db.Users.AsNoTracking().Include(x => x.Posts).Include(x => x.Comments)
        .Include(x => x.Votes).ToListAsync());

app.MapPost("getAllUsers", async ([FromQuery]string nameContains, [FromQuery] int? minPostsAmount,  BasicStackOverflowContext db) =>
{
    minPostsAmount ??= 0;
    
    var usersFiltered = await db.Users.AsNoTracking().Where(x => x.Username.Contains(nameContains) && x.Posts.Count >= minPostsAmount)
        .Select(x => x.Username).ToListAsync();
    
    return usersFiltered.Any() ? Results.Ok(usersFiltered) : Results.NotFound();
});

app.MapGet("getUserQuestions/{id}", async ([FromRoute]int id, BasicStackOverflowContext db) =>
{
    return await db.Questions.AsNoTracking().Where(x => x.AuthorId == id).Include(x => x.Comments)
        .Include(x => x.Answers).Include(x => x.Tags)
        .Select(x => new
        {
            x.Title,
            Question = x.Content,
            Comments = x.Comments.Select(y => new { y.Content, y.CreationDate }),
            Answers = x.Answers.Select(y => new
            {
                y.Content, y.CreationDate,
                Comments = y.Comments.Select(z => new { y.Content, y.Author, y.CreationDate })
            }),
            Tags = x.Tags.Select(y => new { y.Name })
        }).ToListAsync();
});

app.MapGet("getUserAnswers", async ([FromQuery]int id, BasicStackOverflowContext db) =>
{
   var answers = await db.Answers.AsNoTracking()
       .Include(x => x.Question) //.Include(x => x.Question)
        .Where(x => x.Id == id).ToListAsync();
});

app.MapGet("question/{id:int}", async ([FromRoute] int id, IBSOService service) =>
{
    var result = await service.GetQuestion(id);
    return Results.Ok(result);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();