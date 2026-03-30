using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Project
    {
        [Key] 
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Project name is required.")]
        [StringLength(100)]
        public string ProjectName { get; set; } = null!;

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        [Required]
        public int ClientId { get; set; }

        // Navigation property to the related Client
        public Client Client { get; set; } = null!;
    }
}
