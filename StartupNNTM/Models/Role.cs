using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartupNNTM.Models
{
    [Table("Role")]
    public class Role : IdentityRole<Guid>
    {
        public override Guid Id { get => base.Id; set => base.Id = value; }

    }
}
