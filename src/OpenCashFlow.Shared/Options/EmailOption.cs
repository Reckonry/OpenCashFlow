using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Options
{
    public class EmailOption
    {
        public required string From { get; set; }
        public string FromName { get; set; } = "OpenCashFlow";
        public required string SmtpServer { get; set; } = default!;
        public int Port { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
