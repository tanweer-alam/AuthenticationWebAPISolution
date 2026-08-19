using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ClientDto
    {
        [Required(ErrorMessage = "Client name is required.")]
        [StringLength(100, ErrorMessage = "Client name cannot exceed 100 characters.")]
        public string ClientName { get; set; } = null!;

        [Required(ErrorMessage = "Client code is required.")]
        [StringLength(30, ErrorMessage = "Client code cannot exceed 30 characters.")]
        public string ClientCode { get; set; } = null!;
    }
}
