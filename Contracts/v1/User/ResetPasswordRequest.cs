﻿namespace Contracts.v1.User;

public class ResetPasswordRequest
{
    public string Token { get; set; }
    public string Password { get; set; }
}