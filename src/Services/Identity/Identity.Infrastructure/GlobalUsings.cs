global using System;
global using System.Collections.Generic;
global using System.IdentityModel.Tokens.Jwt;
global using System.Linq;
global using System.Security.Claims;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;

global using Identity.Application.Common.Interfaces;

global using Identity.Domain.Entities;
global using Identity.Domain.Interfaces;
global using Identity.Domain.ValueObjects;
global using Identity.Infrastructure.Auth;
global using Identity.Infrastructure.Persistence;
global using Identity.Infrastructure.Persistence.Configurations;
global using Identity.Infrastructure.Persistence.Repositories;
global using Identity.Infrastructure.Redis;
global using Identity.Infrastructure.Services;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using StackExchange.Redis;