using Chattt.Data;
using Chattt.DTOs;
using Chattt.Entities;
using Chattt.Hubs;
using Chattt.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chattt.Services;

public class MessageService(AppDbContext db, IHubContext<ChatHub> hubContext) : IMessageService
{
    public async Task<MessageResult> SendMessageAsync(Guid currentUserId, SendMessageDto dto)
    {
        var sender = await db.Users.FindAsync(currentUserId);
        if (sender is null) return new MessageResult(false, "Sender error");

        if (sender.Username == dto.ReceiverUsername)
        {
            return new MessageResult(false, "Cannot send to self");
        }

        var receiver = await db.Users.SingleOrDefaultAsync(u => u.Username == dto.ReceiverUsername);
        if (receiver is null) return new MessageResult(false, "User not found");

        var message = new Message
        {
            Content = dto.Content,
            SenderId = currentUserId,
            ReceiverId = receiver.Id,
            SentAt = DateTime.UtcNow
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync();

        await hubContext.Clients.User(receiver.Id.ToString())
            .SendAsync("ReceiveMessage", new MessageResponseDto(
                message.Id,
                sender.Username,
                message.Content,
                message.SentAt,
                IsMe: false
            ));

        return new MessageResult(true);
    }

    public async Task<IEnumerable<MessageResponseDto>> GetConversationAsync(Guid currentUserId, string targetUsername)
    {
        var targetUser = await db.Users.SingleOrDefaultAsync(u => u.Username == targetUsername);
        if (targetUser is null) return [];

        return await db.Messages
            .Include(m => m.Sender)
            .Where(m => (m.SenderId == currentUserId && m.ReceiverId == targetUser.Id) ||
                        (m.SenderId == targetUser.Id && m.ReceiverId == currentUserId))
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageResponseDto(
                m.Id,
                m.Sender.Username,
                m.Content,
                m.SentAt,
                m.SenderId == currentUserId
            ))
            .ToListAsync();
    }
}