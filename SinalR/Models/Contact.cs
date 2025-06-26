namespace SinalR.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } 
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public DateTime? DeletedAt { get; set; } 
    }

}
