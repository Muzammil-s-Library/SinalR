namespace SinalR.Models
{
    public class ChatViewModel
    {
        public List<ContactViewModel> Contacts { get; set; }
        public List<Message> Messages { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public DateTime? DeletedAt { get; set; } 
    }
}
