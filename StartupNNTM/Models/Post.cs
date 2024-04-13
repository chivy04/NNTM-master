using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartupNNTM.Models
{
    [Table("Post")]
    public class Post
    {
        [Key]
        public string Id { get; set; }
        public string Title { get; set; }
        public Guid AddressId { get; set; }
        public string Price { get; set; }
        public string Content { get; set; }
        public Guid TypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UserId { get; set; }

        public ICollection<Chat> Chat { get; set; }
        public Type Type { get; set; }
        public Address Address { get; set; }
        public ICollection<Image> Images { get; set; }
    }
}

