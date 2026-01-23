using MimeKit;
using Shared.Models;
using MailKit.Net.Smtp;
using System.Text;
using Shared.Options;
using Shared.Services.Interfaces;

namespace Shared.Services
{
    public class EmailSender(EmailOption emailConfig) : IEmailSender
    {
        private readonly EmailOption _emailConfig = emailConfig;

        public void SendEmail(EmailMessage message, string DestUserName, string DestUserEmail)
        {
            var emailMessage = CreateEmailMessage(message, DestUserName, DestUserEmail);
            Send(emailMessage);
        }
        public async Task SendEmailAsync(EmailMessage message, string DestUserName, string DestUserEmail)
        {
            var mailMessage = CreateEmailMessage(message, DestUserName, DestUserEmail);
            await SendAsync(mailMessage);
        }


        private MimeMessage CreateEmailMessage(EmailMessage message, string DestUserName, string DestUserEmail)
        {
            var emailMessage = new MimeMessage();
            var senderDisplayName = string.IsNullOrWhiteSpace(message.FromName)
                ? _emailConfig.FromName
                : message.FromName;
            emailMessage.From.Add(new MailboxAddress(Encoding.UTF8, senderDisplayName, _emailConfig.From));
            emailMessage.To.Add(new MailboxAddress(Encoding.UTF8, DestUserName, DestUserEmail));
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };
            return emailMessage;
        }


        private void Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    // For Ethereal in development, ignore SSL validation errors
                    if (_emailConfig.SmtpServer.Contains("ethereal.email"))
                    {
                        client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                    }

                    // Proper handling of SSL/TLS options for different providers
                    if (_emailConfig.Port == 465)
                    {
                        // Direct SSL (Aruba)
                        client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    }
                    else if (_emailConfig.Port == 587)
                    {
                        // STARTTLS (Ethereal, Gmail, etc.)
                        client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    }
                    else
                    {
                        // Fallback for other ports
                        client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.Auto);
                    }

                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_emailConfig.UserName, _emailConfig.Password);
                    client.Send(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception or both.
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    // For Ethereal in development, ignore SSL validation errors
                    if (_emailConfig.SmtpServer.Contains("ethereal.email"))
                    {
                        client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                    }

                    // Proper handling of SSL/TLS options for different providers
                    if (_emailConfig.Port == 465)
                    {
                        // Direct SSL (Aruba)
                        await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    }
                    else if (_emailConfig.Port == 587)
                    {
                        // STARTTLS (Ethereal, Gmail, etc.)
                        await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    }
                    else
                    {
                        // Fallback for other ports
                        await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.Auto);
                    }

                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    await client.SendAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EmailSender ERROR] Errore nell'invio email: {ex.Message}");

                    // Specific handling for Aruba errors
                    if (ex.Message.Contains("policy violation"))
                    {
                        if (ex.Message.Contains("rate limit") || ex.Message.Contains("quota"))
                        {
                            Console.WriteLine("[EmailSender ERROR] Rate limit raggiunto - troppi invii email");
                            throw new InvalidOperationException("Email rate limit reached. Please try again later.", ex);
                        }
                        else
                        {
                            Console.WriteLine("[EmailSender ERROR] Email bloccata per policy (dominio o contenuto sospetto)");
                            throw new InvalidOperationException("Email blocked by provider policy. Verify the recipient address.", ex);
                        }
                    }

                    //log an error message or throw an exception, or both.
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
