using Bogus;
using MasterNet.Domain;
using MasterNet.Persistence;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.WebApi.Extensions;

public static class DataSeed
{

  public static async Task SeedDataAuthentication(
      this IApplicationBuilder app
  )
  {
    using var scope = app.ApplicationServices.CreateScope();
    var service = scope.ServiceProvider;
    var loggerFactory = service.GetRequiredService<ILoggerFactory>();

    try
    {
      var context = service.GetRequiredService<MasterNetDbContext>();
      await context.Database.MigrateAsync();
      var userManager = service.GetRequiredService<UserManager<AppUser>>();

      if (!userManager.Users.Any())
      {
        var userAdmin = new AppUser
        {
          NombreCompleto = "Vaxi Drez",
          UserName = "vaxidrez",
          Email = "vaxi.drez@gmail.com"
        };

        await userManager.CreateAsync(userAdmin, "Password123$");
        await userManager.AddToRoleAsync(userAdmin, CustomRoles.ADMIN);

        var userClient = new AppUser
        {
          NombreCompleto = "Juan Perez",
          UserName = "juanperez",
          Email = "juan.perez@gmail.com"
        };

        await userManager.CreateAsync(userClient, "Password123$");
        await userManager.AddToRoleAsync(userClient, CustomRoles.CLIENT);
      }


      // Seed Instructores
      if (!context.Instructores!.Any())
      {
        var fakerInstructor = new Faker<Instructor>()
            .RuleFor(i => i.Id, _ => Guid.NewGuid())
            .RuleFor(i => i.Nombre, f => f.Name.FirstName())
            .RuleFor(i => i.Apellidos, f => f.Name.LastName())
            .RuleFor(i => i.Grado, f => f.Company.CompanyName());

        var instructores = fakerInstructor.Generate(5);
        context.AddRange(instructores);
        await context.SaveChangesAsync();
      }

      // Seed Precios
      if (!context.Precios!.Any())
      {
        var precios = new List<Precio>
        {
          new Precio { Id = Guid.NewGuid(), Nombre = "Básico", PrecioActual = 29.99m, PrecioPromocion = 19.99m },
          new Precio { Id = Guid.NewGuid(), Nombre = "Intermedio", PrecioActual = 49.99m, PrecioPromocion = 39.99m },
          new Precio { Id = Guid.NewGuid(), Nombre = "Avanzado", PrecioActual = 79.99m, PrecioPromocion = 59.99m }
        };
        context.AddRange(precios);
        await context.SaveChangesAsync();
      }

      // Seed Cursos
      if (!context.Cursos!.Any())
      {
        var fakerCurso = new Faker<Curso>()
            .RuleFor(c => c.Id, _ => Guid.NewGuid())
            .RuleFor(c => c.Titulo, f => f.Commerce.ProductName())
            .RuleFor(c => c.Descripcion, f => f.Commerce.ProductDescription())
            .RuleFor(c => c.FechaPublicacion, f => f.Date.Past(yearsToGoBack: 2));

        var cursosCreados = fakerCurso.Generate(10);
        context.AddRange(cursosCreados);
        await context.SaveChangesAsync();
      }

      var cursos = await context.Cursos!.Take(10).Skip(0).ToListAsync();

      if (!context.Set<CursoInstructor>().Any())
      {
        var instructores =
        await context.Instructores!.Take(10).Skip(0).ToListAsync();

        foreach (var curso in cursos)
        {
          curso.Instructores = instructores;
        }
      }

      if (!context.Set<CursoPrecio>().Any())
      {
        var precios = await context.Precios!.ToListAsync();
        foreach (var curso in cursos)
        {
          curso.Precios = precios;
        }
      }

      if (!context.Set<Calificacion>().Any())
      {
        foreach (var curso in cursos)
        {
          var fakerCalificacion = new Faker<Calificacion>()
              .RuleFor(c => c.Id, _ => Guid.NewGuid())
              .RuleFor(c => c.Alumno, f => f.Name.FullName())
              .RuleFor(c => c.Comentario, f => f.Commerce.ProductDescription())
              .RuleFor(c => c.Puntaje, 5)
              .RuleFor(c => c.CursoId, curso.Id);

          var calificaciones = fakerCalificacion.Generate(10);
          context.AddRange(calificaciones);
        }
      }


      await context.SaveChangesAsync();

    }
    catch (Exception e)
    {
      var logger = loggerFactory.CreateLogger<MasterNetDbContext>();
      logger.LogError(e.Message);
    }


  }
}