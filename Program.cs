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
    options.Password.RequiredLength = 8;
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
//builder.Services.AddAuthentication("StudentAuth")
//    .AddCookie("AdminAuth", options =>
//    {
      
//        options.LoginPath = "/Admin/Login";
//        options.AccessDeniedPath = "/Home/AccessDenied";
//    })
//    .AddCookie("TeacherAuth", options =>
//    {
//        options.LoginPath = "/Teacher/LoginTeacher";
//        options.AccessDeniedPath = "/Home/AccessDenied";
//    })
//    .AddCookie("StudentAuth", options =>
//    {
//        options.LoginPath = "/Student/LoginStudent";
//        options.AccessDeniedPath = "/Home/AccessDenied";
//    });
builder.Services.AddAuthorization(options =>
{
    // Optional: define role-based policies (not required if just using roles)
    options.AddPolicy("TeacherOnly", policy =>
        policy.RequireRole("Teacher"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("StudentOnly", policy =>
        policy.RequireRole("Student"));
});

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
builder.Services.AddAuthentication()
    .AddCookie("AdminAuth", options =>
    {
        options.Cookie.Name = "AdminAuth";
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    })
    .AddCookie("TeacherAuth", options =>
    {
        options.Cookie.Name = "TeacherAuth";
        options.LoginPath = "/Teacher/LoginTeacher";
        options.AccessDeniedPath = "/Home/AccessDenied";
    })
    .AddCookie("StudentAuth", options =>
    {
        options.Cookie.Name = "StudentAuth";
        options.LoginPath = "/Student/LoginStudent";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });
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

app.UseRouting();
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
app.MapHub<ChatHub>("/chatHub").RequireAuthorization();



app.Run();
