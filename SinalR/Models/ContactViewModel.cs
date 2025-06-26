namespace SinalR.Models
{
    public class ContactViewModel
    {
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string UserEmail { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnseenCount { get; set; }

    }

}
