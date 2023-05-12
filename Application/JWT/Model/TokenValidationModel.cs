﻿using Domain.DomainEntities;

namespace Application.JWT.Model;

public class TokenValidationModel
{
    public string Id { get; set; }
    public List<UserAccessLevel> AccessLevel { get; set; }
}