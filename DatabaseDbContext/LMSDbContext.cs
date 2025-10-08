using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;



namespace LearningManagementSystem.DatabaseDbContext
{
    public class LMSDbContext:IdentityDbContext<ApplicationUser,ApplicationRole,Guid>
    {
        public LMSDbContext(DbContextOptions options):base(options)
        {
                        
        }
      
        //public virtual DbSet<TeacherDM> TeacherDMs { get; set; }
        public virtual DbSet<TeacherAssignmentDM> AssignmentDMs { get; set; }
        public virtual DbSet<BatchDM> BatchDMs { get; set; }
        public virtual DbSet<StudentAssignmentDM> StudentAssignmentDM { get; set; }

        public virtual DbSet<VideoConference> VideoConference { get; set; }
        public virtual DbSet<StudyMaterialsDM> StudyMaterials { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<StudentAssignmentDM>()
          .HasIndex(sa => new { sa.StudentId, sa.assignmentDMId })
          .IsUnique();
        }
       
        //public DbSet<LearningManagementSystem.Models.DTO.StudentAssignmentVM> StudentAssignmentVM { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.TeacherAssignmentVM> TeacherAssignmentVM { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.RegisterDTO> RegisterDTO { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.LoginDTO> LoginDTO { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.BatchVM> BatchVM { get; set; } = default!;
        





}
}
