using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BasicStackOverflow.Models;

public class CreateUserDTO
{
    [NotNull]
    [Required]
    public string Username { get; set; }
}