using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartupNNTM.Models
{
    [Table("User")]
    public class User : IdentityUser<Guid>
    {
        public string Fullname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool Status { get; set; }
        public ICollection<Message> Message { get; set; }
        public ICollection<Post> Post { get;}
    }
}
