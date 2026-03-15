global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

// Test framework
global using NUnit.Framework;
global using Moq;

// Shared
global using Shared.Domain;

// Domain
global using Identity.Domain.Entities;
global using Identity.Domain.Interfaces;
global using Identity.Domain.ValueObjects;
global using Identity.Domain.Exceptions;

// Application
global using Identity.Application.Common.Errors;
global using Identity.Application.Common.Interfaces;
global using Identity.Application.Users.DTOs;

// Factories
global using Identity.Tests.Factories;
global using Bogus;
