using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Identity
{
    public class AspNetUserDeniedPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Permission { get; set; }
        public virtual AspNetUser? User { get; set; }
    }
}
