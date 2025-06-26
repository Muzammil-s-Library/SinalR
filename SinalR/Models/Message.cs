namespace SinalR.Models
{
    public class Message
{
    public int Id { get; set; }
    public string SenderEmail { get; set; }
    public string ReceiverEmail { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

        public bool IsDelivered { get; set; } = false;
        public bool IsSeen { get; set; } = false;
    }

}
