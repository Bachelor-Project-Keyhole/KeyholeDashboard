﻿using System.ComponentModel.DataAnnotations;
#pragma warning disable CS8618

namespace Service.JWT.Model;

public class AuthenticateRequest
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}