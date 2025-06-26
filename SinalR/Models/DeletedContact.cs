namespace SinalR.Models
{
    public class DeletedContact
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }          // Who deleted the contact
        public string DeletedContactEmail { get; set; } // Whom they deleted
        public DateTime DeletedAt { get; set; }
    }

}
