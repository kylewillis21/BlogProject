using AuthSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class BlogPost
    {
        [Key]
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public string UserId { get; set; } // Foreign Key
        public ApplicationUser User { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
