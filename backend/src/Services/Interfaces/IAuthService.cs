using System.Security.Claims;
using ComprasTccApp.Models.Entities.Pessoas;
using Models.Dtos;

namespace ComprasTccApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Pessoa> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UserProfileDto?> GetMyProfileAsync(ClaimsPrincipal user);
        // Task<bool> ForgotPasswordAsync(string email);
        // Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
