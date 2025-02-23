using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Auth
{
    public class RefreshRequestDto
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
