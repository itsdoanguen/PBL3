using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using PBL3.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using PBL3.Service;

var builder = WebApplication.CreateBuilder(args);

//Get environment variables from .env file
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

//Add IchapterService vao builder
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<BlobService>();
builder.Services.AddScoped<IImageService, ImageService>();
var dbConnectionString = builder.Configuration["DB_CONNECTION_STRING"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(dbConnectionString));
//Them Authorization va Authentication vao builder
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        //Setup duong dan cho login va access denied
        options.LoginPath = "/Authentication/Login";
        options.AccessDeniedPath = "/Authentication/AccessDenied";
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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
