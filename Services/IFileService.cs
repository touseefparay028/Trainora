using LearningManagementSystem.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearningManagementSystem.Services
{
    public interface IFileService
    {
        Task CreateAssignmentAsync(TeacherAssignmentVM assignment);
        Task<List<TeacherAssignmentVM>> GetFilesAsync();
        Task<List<SelectListItem>> GetBatchSelectListAsync();
        Task SubmitAssignmentAsync(StudentAssignmentVM assignmentVM, Guid StudentID);
        Task<List<TeacherAssignmentVM>> GetFilteredFiles();
        Task UploadStudyMaterialAsync(StudyMaterialsVM studyMaterialsVM);
        Task<List<StudyMaterialsVM>> GetMaterialAsync();
        Task<List<StudentAssignmentVM>> SubmittedAssignments(Guid Id);
        Task<List<TeacherAssignmentVM>> GetCreatedAssignments();
    }
}