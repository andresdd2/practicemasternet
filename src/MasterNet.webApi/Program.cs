using MasterNet.Application;
using MasterNet.Application.Interfaces;
using MasterNet.Infrastructure.Photos;
using MasterNet.Infrastructure.Reports;
using MasterNet.Persistence;
using MasterNet.Persistence.Models;
using MasterNet.webApi.Middleware;
using MasterNet.WebApi.Extensions;
using Microsoft.AspNetCore.Identity;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

builder.Services.Configure<CloudinarySettings>
  (builder.Configuration.GetSection(nameof(CloudinarySettings)));

builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddScoped(typeof(IReportService<>), typeof(ReportService<>));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(configuration =>
{
  configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddIdentityCore<AppUser>(opt =>
{
  opt.Password.RequireNonAlphanumeric = false;
  opt.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<MasterNetDbContext>();

builder.Configuration["CloudinarySettings:CloudName"] = 
  Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
builder.Configuration["CloudinarySettings:ApiKey"] =
  Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
builder.Configuration["CloudinarySettings:ApiSecret"] =
  Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

await app.SeedDataAuthentication();

app.MapControllers();
app.Run();