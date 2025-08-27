using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using PBL3.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using PBL3.Service.Chapter;
using PBL3.Service.Image;
using PBL3.Service.Story;
using PBL3.Service.User;
using PBL3.Service.Like;
using PBL3.Service.Discovery;
using PBL3.Service.Style;
using PBL3.Service.Comment;
using PBL3.Service.Follow;
using PBL3.Service.Bookmark;
using PBL3.Service.Notification;
using PBL3.Service.History;
using PBL3.Service.Moderator;
using PBL3.Service.Report;
using PBL3.Service.Admin;
using PBL3.Service.Search;
using PBL3.Service.Dashboard;
using PBL3.Service.Email;

var builder = WebApplication.CreateBuilder(args);

//Get environment variables from .env file
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

//Add IchapterService vao builder
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IStoryQueryService, StoryQueryService>();
builder.Services.AddScoped<BlobService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ILikeChapterService, LikeChapterService>();
builder.Services.AddScoped<IStyleService, StyleService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICommentMappingService, CommentMappingService>();
builder.Services.AddScoped<IStoryRankingService, StoryRankingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportQueryService, ReportQueryService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<IModeratorService, ModeratorService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEmailService, EmailService>();
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
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Localization middleware
var supportedCultures = new[] 
{
    new CultureInfo("vi-VN"),
    new CultureInfo("en-US")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi-VN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
{
    new CookieRequestCultureProvider(),
    new AcceptLanguageHeaderRequestCultureProvider()
};

app.UseRequestLocalization(localizationOptions);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
