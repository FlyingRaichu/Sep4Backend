﻿namespace DatabaseInterfacing.Domain.DTOs;

public class UserValidationDto
{
    public string Username { get; }
    public string Password { get; }

    public UserValidationDto(string username, string password)
    {
        Username = username;
        Password = password;
    }
}