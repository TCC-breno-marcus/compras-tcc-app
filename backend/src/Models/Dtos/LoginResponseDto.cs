using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class LoginResponseDto
    {
        [Required]
        public required string Token { get; set; }

        [Required]
        public required string Message { get; set; }
    }
}
