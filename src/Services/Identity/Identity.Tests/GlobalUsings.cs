global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

// Test framework
global using NUnit.Framework;
global using Moq;

// Shared

// Domain
global using Identity.Domain.Entities;
global using Identity.Domain.Interfaces;
global using Identity.Domain.ValueObjects;
global using Identity.Domain.Exceptions;

// Application
global using Identity.Application.Common.Errors;
global using Identity.Application.Common.Interfaces;
global using Identity.Application.Auth.Commands.Login;
global using Identity.Application.Auth.Commands.Logout;
global using Identity.Application.Invitations.Commands.AcceptInvitation;
global using Identity.Application.Invitations.Commands.CreateInvitation;
global using Identity.Application.Invitations.Commands.ResendInvitation;
global using Identity.Application.PasswordReset.Commands.ChangePassword;
global using Identity.Application.PasswordReset.Commands.RequestPasswordReset;
global using Identity.Application.PasswordReset.Commands.ResetPassword;
global using Identity.Application.Setup.Commands.Bootstrap;
global using Identity.Application.Users.Commands.DeactivateUser;
global using Identity.Application.Users.Commands.ReactivateUser;
global using Identity.Application.Users.Commands.UpdateUserRole;
global using Identity.Application.Users.Queries.GetUserByEmail;
global using Identity.Application.Users.Queries.GetUserById;
global using Identity.Application.Users.Queries.ListUsers;

// Factories
global using Identity.Tests.Factories;
global using Bogus;
