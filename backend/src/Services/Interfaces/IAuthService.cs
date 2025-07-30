using ComprasTccApp.Backend.DTOs;
using ComprasTccApp.Models.Entities.Pessoas;

namespace ComprasTccApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Pessoa> RegisterAsync(RegisterDto registerDto);
        Task<string?> LoginAsync(LoginDto loginDto);
        // Task<bool> ForgotPasswordAsync(string email);
        // Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
