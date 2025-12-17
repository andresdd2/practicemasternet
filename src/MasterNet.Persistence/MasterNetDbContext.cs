using MasterNet.Domain;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MasterNet.Persistence;

public class MasterNetDbContext : IdentityDbContext<AppUser>
{
  public MasterNetDbContext(DbContextOptions<MasterNetDbContext> options) : base(options)
  {
  }

  public MasterNetDbContext()
  {
  }

  public DbSet<Curso>? Cursos { get; set; }
  public DbSet<Instructor>? Instructores { get; set; }
  public DbSet<Precio>? Precios { get; set; }
  public DbSet<Calificacion>? Calificaciones { get; set; }



  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseSqlite("Data Source=LocalDatabase.db")
      .EnableDetailedErrors()
      .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .UseAsyncSeeding(async (context, FtpStatusCode, cancellationToken) =>
    {
      var masterNetDbContext = (MasterNetDbContext)context;
      var logger = context.GetService<ILogger<MasterNetDbContext>>();
      try
      {
        await SeedDatabase.SeedPreciosAsync(
          masterNetDbContext,
          logger,
          cancellationToken
        );

        await SeedDatabase.SeedInstructoresAsync(
          masterNetDbContext,
          logger,
          cancellationToken
        );

        await SeedDatabase.SeedCursosAsync(
          masterNetDbContext,
          logger,
          cancellationToken
        );

        await SeedDatabase.SeedCalificacionesAsync(
          masterNetDbContext,
          logger,
          cancellationToken
        );
      }
      catch (Exception ex)
      {
        logger?.LogError(ex, "Error en el seeding");
      }
    }
    );
  }


  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Curso>().ToTable("cursos");
    modelBuilder.Entity<Instructor>().ToTable("instructores");
    modelBuilder.Entity<CursoInstructor>().ToTable("cursos_instructores");
    modelBuilder.Entity<Precio>().ToTable("precios");
    modelBuilder.Entity<CursoPrecio>().ToTable("cursos_precios");
    modelBuilder.Entity<Calificacion>().ToTable("calificaciones");
    modelBuilder.Entity<Photo>().ToTable("imagenes");

    modelBuilder.Entity<Precio>()
      .Property(b => b.PrecioActual)
      .HasPrecision(10, 2);

    modelBuilder.Entity<Precio>()
      .Property(b => b.PrecioPromocion)
      .HasPrecision(10, 2);

    modelBuilder.Entity<Precio>()
      .Property(b => b.Nombre)
      .HasColumnType("VARCHAR")
      .HasMaxLength(250);

    modelBuilder.Entity<Curso>()
      .HasMany(m => m.Photos)
      .WithOne(m => m.Curso)
      .HasForeignKey(m => m.CursoId)
      .IsRequired()
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<Curso>()
      .HasMany(m => m.Calificaciones)
      .WithOne(m => m.Curso)
      .HasForeignKey(m => m.CursoId)
      .OnDelete(DeleteBehavior.Restrict);

    // Relación muchos a muchos
    modelBuilder.Entity<Curso>()
      .HasMany(m => m.Precios)
      .WithMany(m => m.Cursos)
      .UsingEntity<CursoPrecio>(
        j => j
          .HasOne(p => p.Precio)
          .WithMany(p => p.CursoPrecios)
          .HasForeignKey(p => p.PrecioId),
        j => j
          .HasOne(p => p.Curso)
          .WithMany(p => p.CursoPrecios)
          .HasForeignKey(p => p.CursoId),
        j =>
        {
          j.HasKey(t => new { t.PrecioId, t.CursoId });
        }
      );

    modelBuilder.Entity<Curso>()
      .HasMany(m => m.Instructores)
      .WithMany(m => m.Cursos)
      .UsingEntity<CursoInstructor>(
        j => j
          .HasOne(p => p.Instructor)
          .WithMany(p => p.CursoInstructores)
          .HasForeignKey(p => p.InstructorId),
        j => j
          .HasOne(p => p.Curso)
          .WithMany(p => p.CursoIntructores)
          .HasForeignKey(p => p.CursoId),
        j =>
        {
          j.HasKey(t => new { t.InstructorId, t.CursoId });
        }
      );
  }
}