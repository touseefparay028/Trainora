using LearningManagementSystem.DatabaseDbContext;
using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(Mapper => Mapper.AddMaps(typeof(Program)));
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
    
})
    .AddEntityFrameworkStores<LMSDbContext>().AddDefaultTokenProviders()
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, LMSDbContext, Guid>>()
    .AddRoleStore<RoleStore<ApplicationRole, LMSDbContext,Guid>>();

//builder.Services.AddAuthorization(options =>
//{
//    //options.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
//    options.AddPolicy("NotAuthenticated", Policy =>
//    {
//        Policy.RequireAssertion(context =>
//        {
//            return !context.User.Identity.IsAuthenticated;
//        });
//    });
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

app.UseEndpoints(Endpoints =>
{
    ControllerActionEndpointConventionBuilder controllerActionEndpointConventionBuilder = Endpoints.MapControllers();
});

app.MapControllers(); //  execute endpoint
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
