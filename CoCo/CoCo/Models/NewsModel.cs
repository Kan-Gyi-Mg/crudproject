using System.ComponentModel.DataAnnotations;

namespace CoCo.Models
{
    public class NewsModel
    {
        [Key]
        [Required]
        public int NewsId { get; set; }
        [Required]
        public string NewsTitle { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string NewsBody { get; set; }

        public string? UserId { get; set; }
    }
}
