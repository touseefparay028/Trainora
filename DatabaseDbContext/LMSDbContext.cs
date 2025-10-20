using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;



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
        public virtual DbSet<AccountDeletionReason> AccountDeletionReasons { get; set; }
        public virtual DbSet<VideoConference> VideoConference { get; set; }
        public virtual DbSet<StudyMaterialsDM> StudyMaterials { get; set; }
        public virtual DbSet<CourseDM> Courses { get; set; }
        public virtual DbSet<StudentCourseDM> StudentCourses { get; set; }
        public virtual DbSet<TimeTableDM> TimeTables { get; set; }
        public virtual DbSet<ContactDM> Contact { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<StudentAssignmentDM>()
          .HasIndex(sa => new { sa.StudentId, sa.assignmentDMId })
          .IsUnique();
            builder.Entity<StudentCourseDM>()
            .HasOne(sc => sc.Student)
            .WithMany()
            .HasForeignKey(sc => sc.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentCourseDM>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(sc => sc.CourseId);

            builder.Entity<TimeTableDM>()
                .HasOne(t => t.Course)
                .WithMany(c => c.TimeTables)
                .HasForeignKey(t => t.CourseId);
        }
        //public DbSet<LearningManagementSystem.Models.DTO.CourseVM> CourseVM { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.StudyMaterialsVM> StudyMaterialsVM { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.StudentAssignmentVM> StudentAssignmentVM { get; set; } = default!;
       
        //public DbSet<LearningManagementSystem.Models.DTO.StudentAssignmentVM> StudentAssignmentVM { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.TeacherAssignmentVM> TeacherAssignmentVM { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.RegisterDTO> RegisterDTO { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.LoginDTO> LoginDTO { get; set; } = default!;
        //public DbSet<LearningManagementSystem.Models.DTO.BatchVM> BatchVM { get; set; } = default!;
        





}
}
