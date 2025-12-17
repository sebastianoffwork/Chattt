using System.ComponentModel.DataAnnotations;

namespace Chattt.DTOs;

/*
 * Input.
 */
public record RegisterDto([Required, MinLength(3)] string Username, [Required, MinLength(6)] string Password);
public record LoginDto(string Username, string Password);
public record RefreshTokenRequest(string AccessToken, string RefreshToken);

/*
 * Out.
 */
public record AuthResult(
    bool IsSuccess,
    string? AccessToken = null,
    string? RefreshToken = null,
    string? Error = null
);