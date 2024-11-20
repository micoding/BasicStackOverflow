using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Exceptions;
using BasicStackOverflow.ExtensionMethods;
using BasicStackOverflow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BasicStackOverflow.Services;

public interface IUsersService
{
    Task<GetUserDTO> FindUserByIdAsync(int id);
    Task<IEnumerable<GetUserDTO>> GetAllUsers();
    Task<User> RegisterUser(CreateUserDTO userDto);
    string GenerateJwtToken(LoginUserDTO userDto);
}

public class UsersService : IUsersService
{
    private readonly AuthenticationSettings _authSettings;
    private readonly BasicStackOverflowContext _context;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;
    private ILogger<UsersService> _logger;

    public UsersService(BasicStackOverflowContext context, IMapper mapper, ILogger<UsersService> logger,
        IPasswordHasher<User> passwordHasher, AuthenticationSettings authSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _authSettings = authSettings;
    }

    public async Task<GetUserDTO> FindUserByIdAsync(int id)
    {
        var user = await _context.Users.IncludeForUserDTOs().FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new NotFoundException("Users not found");

        var userDto = _mapper.Map<GetUserDTO>(user);
        return userDto;
    }

    public async Task<IEnumerable<GetUserDTO>> GetAllUsers()
    {
        var users = await _context.Users.IncludeForUserDTOs().ToListAsync();

        if (!users.Any())
            throw new NotFoundException("Users not found");

        var usersDto = _mapper.Map<List<GetUserDTO>>(users);
        return usersDto;
    }

    public async Task<User> RegisterUser(CreateUserDTO userDto)
    {
        var user = _mapper.Map<User>(userDto);

        user.PasswordHash = _passwordHasher.HashPassword(user, userDto.Password);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public string GenerateJwtToken(LoginUserDTO userDto)
    {
        var user = _context.Users
            .Include(x => x.Role)
            .FirstOrDefault(x => x.Username == userDto.Username);

        if (user is null)
            throw new BadRequestException("Invalid username or password");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userDto.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new BadRequestException("Invalid username or password");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.Name)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now + TimeSpan.FromDays(_authSettings.JwtExpireDays);

        var token = new JwtSecurityToken(_authSettings.JwtIssuer,
            _authSettings.JwtIssuer,
            claims,
            expires: expires,
            signingCredentials: creds);

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    public async Task Login()
    {
    }
}