using System.ComponentModel.DataAnnotations;

namespace GraspService.Models
{
    public class View
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string SqlScript { get; set; }
    }
}