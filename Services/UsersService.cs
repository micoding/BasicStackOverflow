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
    Task<GETUserDTO> FindUserByIdAsync(int id);
    Task<IEnumerable<GETUserDTO>> GetAllUsers();
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

    public async Task<GETUserDTO> FindUserByIdAsync(int id)
    {
        var user = await _context.Users.GetUserDTOs().FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new NotFoundException("Users not found");

        var userDto = _mapper.Map<GETUserDTO>(user);
        return userDto;
    }

    public async Task<IEnumerable<GETUserDTO>> GetAllUsers()
    {
        var users = await _context.Users.GetUserDTOs().ToListAsync();

        if (!users.Any())
            throw new NotFoundException("Users not found");

        var usersDto = _mapper.Map<List<GETUserDTO>>(users);
        return usersDto;
    }
}