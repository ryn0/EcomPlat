namespace EcomPlat.Email.Models
{
    public class SendGridConfig
    {
        required public string ApiKey { get; set; }
        required public string SenderEmail { get; set; }
        required public string SenderName { get; set; }
    }
}
