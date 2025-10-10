using LearningManagementSystem.ChatHubs;
using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.IdentityEntities;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(Mapper => Mapper.AddMaps(typeof(Program)));
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddDbContext<LMSDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevCon"));
});
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 10;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = true; // important!

})
    .AddEntityFrameworkStores<LMSDbContext>().AddDefaultTokenProviders()
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, LMSDbContext, Guid>>()
    .AddRoleStore<RoleStore<ApplicationRole, LMSDbContext,Guid>>();
builder.Services.AddSignalR();

//builder.Services.AddAuthorization(options =>

//{    //Global Authorization...
//    //options.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
//  options.AddPolicy("NotAuthenticated", Policy =>
//  {
//       Policy.RequireAssertion(context =>
//        {
//          if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
//          {
//          context.Fail();
//          return false;
//           }
//             return true;

//        }); 
//  });
//});
//Custom AccessDenied Path
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.AccessDeniedPath = "/Home/AccessDenied";
//});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();// enable select endpoint
app.UseAuthentication();
app.UseAuthorization();

//app.UseEndpoints(Endpoints =>
//{
//    ControllerActionEndpointConventionBuilder controllerActionEndpointConventionBuilder = Endpoints.MapControllers();
//});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers(); //  execute endpoint
app.MapHub<ChatHub>("/chatHub");



app.Run();
