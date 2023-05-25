using System.ComponentModel.DataAnnotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618

namespace Contracts.v1.Authentication;

public class AuthenticateRequest
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}