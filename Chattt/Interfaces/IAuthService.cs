using Chattt.DTOs;

namespace Chattt.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request);
}