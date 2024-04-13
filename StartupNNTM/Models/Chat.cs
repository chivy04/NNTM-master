using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartupNNTM.Models
{
    [Table("Chat")]
    public class Chat
    {
        [Key]
        public Guid Id { get; set; }
        public string PostId { get; set; }
        public string? Title { get; set; }
        public string Image {  get; set; }
        public Guid MessageId { get; set; }
        public Message Message { get; set; }
        public Post Post { get; set; }
    }
}
