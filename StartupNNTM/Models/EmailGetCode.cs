using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartupNNTM.Models
{
    [Table("EmailGetCode")]
    public class EmailGetCode
    {
        [Key]
        public Guid Id { get; set; }
        public string EmailName { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
