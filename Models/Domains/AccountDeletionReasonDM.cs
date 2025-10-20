namespace LearningManagementSystem.Models.Domains
{
    public class AccountDeletionReasonDM
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Reason { get; set; }
        public DateTime DeletedAt { get; set; }

       
    }
}
