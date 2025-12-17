using System.Security.Claims;
using Chattt.DTOs;
using Chattt.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chattt.Endpoints;

public static class MessageEndpoints
{
    public static void MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/messages")
            .WithTags("Messages")
            .RequireAuthorization();

        group.MapPost("/", async ([FromBody] SendMessageDto dto, IMessageService service, ClaimsPrincipal user) =>
        {
            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            var result = await service.SendMessageAsync(userId, dto);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(new { result.Error });
        })
        .WithName("SendMessage")
        .WithSummary("Send message to a user")
        .Produces(200)
        .Produces(400)
        .Produces(401);

        group.MapGet("/{targetUsername}", async (string targetUsername, IMessageService service, ClaimsPrincipal user) =>
        {
            var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            var history = await service.GetConversationAsync(userId, targetUsername);
            return Results.Ok(history);
        })
        .WithName("GetConversation")
        .WithSummary("Get message history with a concrete user")
        .Produces<IEnumerable<MessageResponseDto>>(200)
        .Produces(401);
    }
}