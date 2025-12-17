using MasterNet.Domain;
using MasterNet.Persistence;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging(l =>
{
  l.ClearProviders();
});

services.AddDbContext<MasterNetDbContext>();

services.AddIdentityCore<AppUser>(options =>
{
  options.Password.RequiredLength = 6;
});

var provider = services.BuildServiceProvider();

try
{
  using var scope = provider.CreateScope();
  var context = scope.ServiceProvider.GetRequiredService<MasterNetDbContext>();
  await context.Database.MigrateAsync();
  Console.WriteLine("La migración y el seeding estan completados");

  var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
  var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
  var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
              .CreateLogger("SeedIdentity");
              
  await SeedDatabase.SeedRolesAndUsersAsync(
    userManager,
    roleManager,
    logger,
    CancellationToken.None
  );            
}
catch (Exception ex)
{
  Console.WriteLine($"Error en el seeding/migration {ex.Message}");
}