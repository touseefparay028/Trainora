namespace LearningManagementSystem.Models.DTO
{
    public class AccountDeletionReason
    {
       public Guid id { get; set; }=Guid.NewGuid();
        public string Reason { get; set; }
        

    }
}
