using System.Collections.Frozen;
using System.Security.Claims;
using MasterNet.Domain;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MasterNet.Persistence;

public static class SeedDatabase
{

  public static async Task SeedRolesAndUsersAsync(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger? logger,
    CancellationToken cancellationToken
  )
  {
    try
    {
      
      
      if (userManager.Users.Any()) return;
      
      var adminId = "dbfda079-caf1-4dc1-8ee4-15a8e4246084";
      var clientId = "681d0c02-5b4e-4d22-85b6-1a84aa4b3c39";
      
      var roleAdmin = new IdentityRole
      {
        Id = adminId,
        Name = CustomRoles.ADMIN,
        NormalizedName = CustomRoles.ADMIN.ToUpperInvariant()
      };
      
      var roleClient = new IdentityRole
      {
        Id = clientId,
        Name = CustomRoles.CLIENT,
        NormalizedName = CustomRoles.CLIENT.ToUpperInvariant()
      };
      
      if (!await roleManager.RoleExistsAsync(CustomRoles.ADMIN))
      {
        await roleManager.CreateAsync(roleAdmin);
      }
      
      if (!await roleManager.RoleExistsAsync(CustomRoles.CLIENT))
      {
        await roleManager.CreateAsync(roleClient);
      }
      
      var userAdmin = new AppUser
      {
        NombreCompleto = "Andres Diaz",
        UserName = "andresadmin",
        Email = "andresadmin@example.com"
      };
      
      await userManager.CreateAsync(userAdmin, "Password123$");
      
      var userClient = new AppUser
      {
        NombreCompleto = "Andres Diaz",
        UserName = "andresclient",
        Email = "andresclient@example.com"
      };
      
      await userManager.CreateAsync(userClient, "Password123$");
      
      // Agregar un determinado role a cada usuario
      await userManager.AddToRoleAsync(userAdmin, CustomRoles.ADMIN);
      await userManager.AddToRoleAsync(userClient, CustomRoles.CLIENT);
      
      // Agregar a cada role su lista de custom claims (policies)
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.CURSO_READ
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.CURSO_UPDATE
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.CURSO_WRITE
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.CURSO_DELETE
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.CURSO_DELETE
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.INSTRUCTOR_READ
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.INSTRUCTOR_UPDATE
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.COMENTARIO_READ
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.COMENTARIO_DELETE
      ));
      
      await roleManager.AddClaimAsync(
        roleAdmin, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.COMENTARIO_CREATE
      ));
      
      await roleManager.AddClaimAsync(
        roleClient, 
        new Claim(CustomClaims.POLICIES, PolicyMaster.COMENTARIO_READ
      ));

      await roleManager.AddClaimAsync(
        roleClient,
        new Claim(CustomClaims.POLICIES, PolicyMaster.INSTRUCTOR_READ
      ));

      await roleManager.AddClaimAsync(
        roleClient,
        new Claim(CustomClaims.POLICIES, PolicyMaster.CURSO_READ
      ));
      
      await roleManager.AddClaimAsync(
        roleClient,
        new Claim(CustomClaims.POLICIES, PolicyMaster.COMENTARIO_CREATE
      ));
      
    } catch (Exception ex)
    {
      logger?.LogWarning(ex, "Fallo en el proceso de identity seed");
    }
  }

  public static async Task SeedPreciosAsync(
    MasterNetDbContext dbContext,
    ILogger? logger,
    CancellationToken cancellationToken
  )
  {
    try
    {
      if (dbContext.Precios is null || dbContext.Precios.Any()) return;
      var jsonString = GetJsonFile("precios.json");

      var precios = JsonConvert.DeserializeObject<List<Precio>>(jsonString);

      if (precios is null || precios?.Any() == false) return;

      dbContext.Precios.AddRange(precios!);
      await dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      logger?.LogWarning(ex, "Fallo cargando la data de precios");
    }
  }

  public static async Task SeedInstructoresAsync(
      MasterNetDbContext dbContext,
      ILogger? logger,
      CancellationToken cancellationToken
  )
  {
    try
    {
      if (dbContext.Instructores is null || dbContext.Instructores.Any()) return;
      var jsonString = GetJsonFile("instructores.json");

      var instructores = JsonConvert.DeserializeObject<List<Instructor>>(jsonString);

      if (instructores is null || instructores?.Any() == false) return;

      dbContext.Instructores.AddRange(instructores!);
      await dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      logger?.LogWarning(ex, "Fallo cargando la data de instructores");
    }
  }

  public static async Task SeedCursosAsync(
      MasterNetDbContext dbContext,
      ILogger? logger,
      CancellationToken cancellationToken
  )
  {
    try
    {
      if (dbContext.Cursos is null || dbContext.Cursos.Any()) return;
      var jsonString = GetJsonFile("cursos.json");

      var instructores = dbContext
                         .Instructores!
                         .ToFrozenDictionary(p => p.Id, p => p);

      var precios = dbContext
                         .Precios!
                         .ToFrozenDictionary(p => p.Id, p => p);

      var arrayCursos = JArray.Parse(jsonString);

      var cursosDb = new List<Curso>();

      foreach (var obj in arrayCursos)
      {
        var idString = obj["Id"]?.ToString();
        if (!Guid.TryParse(idString, out var id))
          id = Guid.NewGuid();

        var titulo = obj["Titulo"]?.ToString();
        var descripcion = obj["Descripcion"]?.ToString();

        DateTime? fechaPublicacion = null;
        var fechaPublicacionStr = obj["FechaPublicacion"]?.ToString();

        if (!string.IsNullOrWhiteSpace(fechaPublicacionStr) && DateTime.TryParse(fechaPublicacionStr, out var fp))
        {
          fechaPublicacion = fp;
        }

        var curso = new Curso
        {
          Id = id,
          Titulo = titulo,
          Descripcion = descripcion,
          FechaPublicacion = fechaPublicacion,
          Calificaciones = new List<Calificacion>(),
          Precios = new List<Precio>(),
          CursoPrecios = new List<CursoPrecio>(),
          Instructores = new List<Instructor>(),
          CursoIntructores = new List<CursoInstructor>(),
          Photos = new List<Photo>(),
        };

        if (obj["Precios"] is JArray preciosC)
        {
          foreach (var pid in preciosC)
          {
            var idt = new Guid(pid?.ToString()!);
            if (precios.TryGetValue(idt, out var precio))
            {
              curso.Precios.Add(precio);
            }
          }
        }

        if (obj["Instructores"] is JArray instructoresC)
        {
          foreach (var iid in instructoresC)
          {
            var idt = new Guid(iid?.ToString()!);
            if (instructores.TryGetValue(idt, out var instructor))
            {
              curso.Instructores.Add(instructor);
            }
          }
        }

        cursosDb.Add(curso);
      }

      await dbContext.Cursos.AddRangeAsync(cursosDb);
      await dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      logger?.LogWarning(ex, "Fallo cargando la data de cursos");
    }
  }

  public static async Task SeedCalificacionesAsync(
      MasterNetDbContext dbContext,
      ILogger? logger,
      CancellationToken cancellationToken
  )
  {
    try
    {
      if (dbContext.Calificaciones is null || dbContext.Calificaciones.Any()) return;
      var jsonString = GetJsonFile("calificaciones.json");

      var calificaciones = JsonConvert.DeserializeObject<List<Calificacion>>(jsonString);

      if (calificaciones is null || calificaciones?.Any() == false) return;

      foreach (var ca in calificaciones!)
      {
        ca.Curso = null;
      }

      dbContext.Calificaciones.AddRange(calificaciones!);
      await dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      logger?.LogWarning(ex, "Fallo cargando la data de calificaciones");
    }
  }

  private static string GetJsonFile(string fileName)
  {
    var leerForma1 = Path.Combine(
        Directory.GetCurrentDirectory(),
        "src",
        "MasterNet.Persistence",
        "SeedData",
        fileName
    );

    var leerForma2 = Path.Combine(
        Directory.GetCurrentDirectory(),
        "SeedData",
        fileName
    );

    var leerForm3 = Path.Combine(
        AppContext.BaseDirectory,
        "SeedData",
        fileName
    );

    if (File.Exists(leerForma1)) return File.ReadAllText(leerForma1);
    if (File.Exists(leerForma2)) return File.ReadAllText(leerForma2);
    if (File.Exists(leerForm3)) return File.ReadAllText(leerForm3);

    return null!;
  }
}