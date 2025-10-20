using AutoMapper;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.MappingProfiles
{
    public class LMSDBMappingProfile:Profile
    {
        public LMSDBMappingProfile()
        {
            CreateMap<RegisterDTO, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForSourceMember(drc => drc.ConfirmPassword, opt => opt.DoNotValidate());



            CreateMap<TeacherAssignmentVM, TeacherAssignmentDM>()
            .ForMember(dest => dest.Path, opt => opt.Ignore());  //File is Saved seperately
            CreateMap<TeacherAssignmentDM, TeacherAssignmentVM>();
            CreateMap<BatchVM, BatchDM>().ReverseMap();
            CreateMap<StudyMaterialsDM, StudyMaterialsVM>().ReverseMap();
            CreateMap<StudentAssignmentVM, StudentAssignmentDM>().ReverseMap();
            CreateMap<CourseDM, CourseVM>().ReverseMap();
            CreateMap<TimeTableDM, TimeTableVM>().ReverseMap();
            CreateMap<AccountDeletionReason, AccountDeletionReasonDM>().ReverseMap();
        }
    }
}
