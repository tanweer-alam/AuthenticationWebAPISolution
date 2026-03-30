using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class ClientSecret
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClientId { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string SecretKey { get; set; } = null!;
    }
}
