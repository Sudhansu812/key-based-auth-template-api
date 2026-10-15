namespace KeyBasedAuthenticationTemplate_Contracts.Notifications
{
    public class SendEmailResponse
    {
        public Guid MessageId { get; set; }
        public DateTime AcceptedAtUtc { get; set; }
    }
}
