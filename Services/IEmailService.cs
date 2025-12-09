using LearningManagementSystem.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Services
{
    public interface IEmailService
    {
            Task SendMail(string TO= "jetbrain028@gmail.com", 
            string Subject = "Registration Successful",
            string Body = "Dear User,\r\n\r\nWelcome to TrainOra 🎉\r\n\r\nWe’re excited to have you join us! Your account has been successfully.\r\n\r\nWith TrainOra, you can:\r\n- Access personalized learning and training resources\r\n- Track your progress and achievements\r\n- Stay updated with new courses and features\r\n\r\nYou can now log in anytime using your registered email.\r\n\r\nIf you did not register for TrainOra, please ignore this email.\r\n\r\nBest regards,  \r\nThe TrainOra Team\r\n", 
            bool ishtml = false);
        Task SendVerificationEmailAsync(string email, string subject, string htmlMessage);
        Task SendResetLinkAsync(string email, string subject, string htmlMessage);
    }
}