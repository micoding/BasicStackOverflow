using AutoMapper;
using BasicStackOverflow.Entities;
using BasicStackOverflow.Exceptions;
using BasicStackOverflow.ExtensionMethods;
using BasicStackOverflow.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BasicStackOverflow.Services;

public interface IUsersService
{
    Task<GetUserDTO> FindUserByIdAsync(int id);
    Task<IEnumerable<GetUserDTO>> GetAllUsers();
}

public class UsersService : IUsersService
{
    private readonly BasicStackOverflowContext _context;
    private ILogger<UsersService> _logger;
    private readonly IMapper _mapper;

    public UsersService(BasicStackOverflowContext context, IMapper mapper, ILogger<UsersService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
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
}