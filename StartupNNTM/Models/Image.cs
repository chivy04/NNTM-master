using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartupNNTM.Models
{
    [Table("Image")]
    public class Image
    {
        [Key]
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string PostId { get; set; }

        public Post Post { get; set; }
    }
}
