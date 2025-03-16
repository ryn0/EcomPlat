using EcomPlat.Email.Models;
using EcomPlat.Email.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EcomPlat.Email.Services.Implementaions
{
    public class EmailService : IEmailService
    {
        private readonly SendGridClient client;
        private readonly string senderEmail;
        private readonly string senderName;
        private readonly bool useUnsubscribeHeader = false;
        private readonly string unsubscribeEmail = string.Empty;
        private readonly string unsubscribeUrlFormat = string.Empty;
        private readonly SendGridConfig config;

        public EmailService(SendGridConfig config)
        {
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                throw new ArgumentException("SendGrid API Key is missing in configuration");
            }

            this.client = new SendGridClient(config.ApiKey);
            this.senderEmail = config.SenderEmail;
            this.senderName = config.SenderName;

            if (this.config != null &&
                !string.IsNullOrWhiteSpace(this.config.UnsubscribeEmail) &&
                !string.IsNullOrWhiteSpace(this.config.UnsubscribeUrlFormat))
            {
                this.unsubscribeEmail = this.config.UnsubscribeEmail.Trim();
                this.unsubscribeUrlFormat = this.config.UnsubscribeUrlFormat.Trim();
                this.useUnsubscribeHeader = true;
            }
        }

        public async Task SendEmailAsync(string subject, string plainTextContent, string htmlContent, List<string> recipients)
        {
            if (recipients == null || recipients.Count == 0)
            {
                throw new ArgumentException("Recipient list cannot be empty", nameof(recipients));
            }

            var from = new EmailAddress(this.senderEmail, this.senderName);
            var msg = new SendGridMessage
            {
                From = from,
                Subject = subject,
                PlainTextContent = plainTextContent,
                HtmlContent = htmlContent
            };

            if (recipients.Count == 1)
            {
                var recipient = recipients[0];
                msg.AddTo(new EmailAddress(recipient));
                this.AddUnsubscribeHeaders(recipient, msg);
            }
            else
            {
                // Add recipients as BCC for bulk sending
                var emailAddresses = recipients.Select(recipient => new EmailAddress(recipient)).ToList();
                msg.AddBccs(emailAddresses);

                // todo: add unsub for here
            }

            try
            {
                var response = await this.client.SendEmailAsync(msg);
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(await response.Body.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        public void AddUnsubscribeHeaders(string recipientEmail, SendGridMessage msg)
        {
            if (!this.useUnsubscribeHeader)
            {
                return;
            }

            // Generate unsubscribe link using the dynamic recipient email
            var unsubscribeUrl = string.Format(this.unsubscribeUrlFormat, Uri.EscapeDataString(recipientEmail));

            // Add List-Unsubscribe header with both mailto and HTTPS options
            msg.AddHeader("List-Unsubscribe", $"<mailto:{this.unsubscribeEmail}>, <{unsubscribeUrl}>");

            // Add List-Unsubscribe-Post header for one-click unsubscribe
            msg.AddHeader("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");
        }
    }
}
