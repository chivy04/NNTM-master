using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartupNNTM.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
        public Guid From { get; set; }
        public Guid To { get; set; }
        public bool WasRed { get; set; }


        public User User { get; set; }
        public ICollection<Chat> Chat { get; set; }

    }
}
