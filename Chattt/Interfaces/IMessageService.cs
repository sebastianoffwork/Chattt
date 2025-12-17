using Chattt.DTOs;

namespace Chattt.Interfaces;

public interface IMessageService
{
    Task<MessageResult> SendMessageAsync(Guid currentUserId, SendMessageDto dto);
    Task<IEnumerable<MessageResponseDto>> GetConversationAsync(Guid currentUserId, string targetUsername);
}