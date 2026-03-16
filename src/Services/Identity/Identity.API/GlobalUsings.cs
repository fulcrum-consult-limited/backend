global using System;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Security.Claims;
global using System.Text;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.IdentityModel.Tokens;

global using Shared.Domain;

global using Identity.Domain.Interfaces;
global using Identity.Domain.Entities;

global using Identity.Application.Common.Interfaces;
global using Identity.Application.Users.Queries.GetUserById;
global using Identity.Application.Users.Queries.GetUserByEmail;
global using Identity.Application.Users.Queries.ListUsers;
global using Identity.Application.Users.Commands.DeactivateUser;
global using Identity.Application.Users.Commands.ReactivateUser;
global using Identity.Application.Users.Commands.UpdateUserRole;
global using Identity.Application.Invitations.Commands.CreateInvitation;
global using Identity.Application.Invitations.Commands.AcceptInvitation;
global using Identity.Application.Invitations.Commands.ResendInvitation;
global using Identity.Application.Setup.Commands.Bootstrap;
global using Identity.Application.Invitations.Queries.GetInvitationByToken;
global using Identity.Application.Invitations.Queries.ListPendingInvitations;
global using Identity.Application.Auth.Commands.Login;
global using Identity.Application.Auth.Commands.Logout;
global using Identity.Application.PasswordReset.Commands.RequestPasswordReset;
global using Identity.Application.PasswordReset.Commands.ResetPassword;
global using Identity.Application.PasswordReset.Commands.ChangePassword;

global using Identity.API.Extensions;
global using Identity.API.Middleware;
global using Identity.Infrastructure;