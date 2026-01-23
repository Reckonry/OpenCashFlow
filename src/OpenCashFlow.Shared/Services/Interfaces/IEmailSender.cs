using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services.Interfaces
{
    public interface IEmailSender
    {
        void SendEmail(EmailMessage message, string DestUserName, string DestUserEmail);
        Task SendEmailAsync(EmailMessage message, string DestUserName, string DestUserEmail);
    }
}
