using System.ComponentModel.DataAnnotations;

namespace Chattt.DTOs;

/*
 * Input.
 */
public record SendMessageDto(
    [Required] string ReceiverUsername,
    [Required, MinLength(1), MaxLength(2000)] string Content
);
public record MessageResponseDto(
    Guid Id,
    string SenderUsername,
    string Content,
    DateTime SentAt,
    bool IsMe
);

/*
 * Out.
 */
public record MessageResult(bool IsSuccess, string? Error = null);