using AuthSystem.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public int PostId { get; set; } // Foreign Key
        public BlogPost Post { get; set; }

        public string UserId { get; set; } // Foreign Key
        public ApplicationUser User { get; set; }
    }
}
