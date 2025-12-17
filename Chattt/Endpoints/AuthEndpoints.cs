using Chattt.DTOs;
using Chattt.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chattt.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async ([FromBody] RegisterDto dto, IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(dto);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("Register")
        .WithSummary("Create a new user") 
        .Produces<AuthResult>(200)
        .Produces<AuthResult>(400);

        group.MapPost("/login", async ([FromBody] LoginDto dto, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(dto);
            return result.IsSuccess ? Results.Ok(result) : Results.Unauthorized();
        })
        .WithName("Login")
        .WithSummary("Log in")
        .Produces<AuthResult>(200)
        .Produces(401);

        group.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.Unauthorized();
        })
        .WithName("Refresh")
        .WithSummary("Update Access token")
        .Produces<AuthResult>(200)
        .Produces(401);
    }
}