using LearningManagementSystem.Models.IdentityEntities;

namespace LearningManagementSystem.Models.Domains
{
    public class StudentAssignmentDM
    {
        public Guid Id { get; set; }
        public string StudentName { get; set; }
        public string Path  { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public bool IsReverted { get; set; }
        //Foriegn Keys
        public Guid assignmentDMId { get; set; }
        public TeacherAssignmentDM assignmentDM { get; set; }
        public Guid StudentId { get; set; }
        public ApplicationUser Student {  get; set; }

    }
}
