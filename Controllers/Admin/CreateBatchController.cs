using AutoMapper;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.Domains;
using LearningManagementSystem.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers.Admin
{
    public class CreateBatchController : Controller
    {
        private readonly LMSDbContext lMSDbContext;
        private readonly IMapper mapper;

        public CreateBatchController(LMSDbContext lMSDbContext, IMapper mapper)
        {
            this.lMSDbContext = lMSDbContext;
            this.mapper = mapper;
        }
        [Route("CreateBatch")]
        [Authorize(AuthenticationSchemes ="AdminAuth",Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost("PostCreateBatch")]
        [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]
        public async Task<IActionResult> CreateBatch(BatchVM batchVM)
        {
           BatchDM batchDM = mapper.Map<BatchDM>(batchVM);
            await lMSDbContext.BatchDMs.AddAsync(batchDM);
            await lMSDbContext.SaveChangesAsync();
            return RedirectToAction("Read");
        }
        [Route("GetBatch")]
        [Authorize(AuthenticationSchemes ="AdminAuth",Roles ="Admin")]
        public async Task<IActionResult> Read()
        {
            List<BatchDM> batchDMs = await lMSDbContext.BatchDMs.ToListAsync();
            List<BatchVM> batchVM = mapper.Map<List<BatchVM>>(batchDMs);
            return View(batchVM);
        }
        [Authorize(AuthenticationSchemes ="AdminAuth",Roles ="Admin")]
        public IActionResult Delete(Guid id)
        {
            // 1. Fetch from DM (database)
            var BatchDM = lMSDbContext.BatchDMs.FirstOrDefault(a => a.id == id);
            if (BatchDM == null)
            {
                ModelState.AddModelError(string.Empty, "Not Found");
                return View("Read", BatchDM);
            }

          

            // 3. Remove record from DB
            lMSDbContext.BatchDMs.Remove(BatchDM);
            lMSDbContext.SaveChanges();

            // 4. Redirect back to List of the assignments.
            return RedirectToAction("Read");
        }
    }
}
