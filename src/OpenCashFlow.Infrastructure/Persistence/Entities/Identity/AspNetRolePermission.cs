using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Infrastructure.Persistence.Entities.Identity
{
    public class AspNetRolePermission
    {    
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string? Permission { get; set; }
        public virtual AspNetRole? Role { get; set; }
    }
}
