global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Shared.Domain;
global using Shared.Application;
global using Identity.Application.Common.Interfaces;

global using Identity.Domain.Entities;
global using Identity.Domain.Interfaces;
global using Identity.Domain.ValueObjects;
global using Identity.Infrastructure.Persistence;
global using Identity.Infrastructure.Services;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;