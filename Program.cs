using System.Text;
using System.Text.Json.Serialization;
using BasicStackOverflow;
using BasicStackOverflow.Authorization;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Filters;
using BasicStackOverflow.Middleware;
using BasicStackOverflow.Models;
using BasicStackOverflow.Models.Validators;
using BasicStackOverflow.Requests;
using BasicStackOverflow.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
builder.Services.AddScoped<IAuthorizationHandler, PostOperationRequirementHandler>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IValidator<PagesQuery>, PagedQueryValidator>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClinet", build =>
    {
        build.AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins(builder.Configuration["AllowedOrigins"]);
    });
});

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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy =>
        policy
            .RequireRole("Admin"))
    .AddPolicy("LogedIn", policy => { policy.RequireAuthenticatedUser(); });

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
builder.Logging.ClearProviders();
builder.Host.UseNLog();


var app = builder.Build();

app.UseCors("FrontendClinet");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestTimeMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<BasicStackOverflowContext>();
DataGenerator.Seed(dbContext);

app.MapGet("question/{id:int}", QuestionRequests.GetById);

app.MapGet("questions", QuestionRequests.GetAll).AddEndpointFilter<ValidationFilter<PagesQuery>>()
    .ProducesValidationProblem();

app.MapDelete("question/{id:int}", QuestionRequests.Delete);

app.MapPatch("question/{id:int}/answer/{answerId:int}/markBest", QuestionRequests.MarkAnswered)
    .RequireAuthorization("LogedIn");

app.MapPost("question/new", QuestionRequests.Create).RequireAuthorization("LogedIn");

app.MapPost("question/{id:int}/answer", QuestionRequests.AddAnswer).RequireAuthorization("LogedIn");


app.MapGet("users", UserRequests.GetAll);

app.MapGet("user/{id:int}", UserRequests.GetById);

app.MapDelete("user/{id:int}", UserRequests.DeletebyId);

app.MapPost("users/register", UserRequests.Register);

app.MapPost("users/login", UserRequests.LogIn);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();