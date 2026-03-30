using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace API.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [StringLength(100, ErrorMessage = "Client name cannot exceed 100 characters.")]
        public string ClientName { get; set; } = null!;

        [Required(ErrorMessage = "Client code is required.")]
        [StringLength(30, ErrorMessage = "Client code cannot exceed 30 characters.")]
        public string ClientCode { get; set; } = null!;

        // Navigation property: initialize collection so it's never null
        public ICollection<Project> Projects { get; set; } = new List<Project>();

    }
}
