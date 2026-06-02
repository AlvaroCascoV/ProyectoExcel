using Attendance.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RoleTajamar> RolesTajamar => Set<RoleTajamar>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<TajamarUser> TajamarUsers => Set<TajamarUser>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<CourseCalendarEntry> CourseCalendarEntries => Set<CourseCalendarEntry>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.TajamarUserId).IsRequired();
            entity.HasIndex(u => u.TajamarUserId).IsUnique();
        });

        builder.Entity<RoleTajamar>(entity =>
        {
            entity.ToTable("ROLESCHARLASTAJAMAR", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("IDROLE");
            entity.Property(e => e.Name).HasColumnName("ROLE").HasMaxLength(100);
        });

        builder.Entity<Course>(entity =>
        {
            entity.ToTable("CURSOSTAJAMAR", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("IDCURSO");
            entity.Property(e => e.Name).HasColumnName("NOMBRE").HasMaxLength(150);
            entity.Property(e => e.StartDate).HasColumnName("FECHAINICIO");
            entity.Property(e => e.EndDate).HasColumnName("FECHAFIN");
            entity.Property(e => e.IsActive).HasColumnName("ACTIVO");
        });

        builder.Entity<TajamarUser>(entity =>
        {
            entity.ToTable("USUARIOSTAJAMAR", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("IDUSUARIO");
            entity.Property(e => e.FirstName).HasColumnName("NOMBRE").HasMaxLength(70);
            entity.Property(e => e.LastName).HasColumnName("APELLIDOS").HasMaxLength(70);
            entity.Property(e => e.Email).HasColumnName("EMAIL").HasMaxLength(70);
            entity.Property(e => e.IsActive).HasColumnName("ESTADO");
            entity.Property(e => e.ImageUrl).HasColumnName("IMAGEN").HasMaxLength(600);
            entity.Property(e => e.LegacyPassword).HasColumnName("PASSWORD").HasMaxLength(100);
            entity.Property(e => e.RoleId).HasColumnName("IDROLE");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId);
        });

        builder.Entity<CourseEnrollment>(entity =>
        {
            entity.ToTable("CURSOSUSUARIOSTAJAMAR", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("IDCURSOSUSUARIOS");
            entity.Property(e => e.CourseId).HasColumnName("IDCURSO");
            entity.Property(e => e.UserId).HasColumnName("IDUSUARIO");

            entity.HasIndex(e => new { e.CourseId, e.UserId }).IsUnique();

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId);
        });

        builder.Entity<AttendanceRecord>(entity =>
        {
            entity.ToTable("ASISTENCIATAJAMAR", t => t.ExcludeFromMigrations());
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("IDASISTENCIA");
            entity.Property(e => e.StudentId).HasColumnName("IDUSUARIO");
            entity.Property(e => e.CourseId).HasColumnName("IDCURSO");
            entity.Property(e => e.Date).HasColumnName("FECHA");
            entity.Property(e => e.Status).HasColumnName("ESTADO");
            entity.Property(e => e.Comment).HasColumnName("COMENTARIO").HasMaxLength(500);
            entity.Property(e => e.RecordedByUserId).HasColumnName("IDPROFESOR");
            entity.Property(e => e.RecordedAt).HasColumnName("FECHAREGISTRO");

            entity.HasIndex(e => new { e.StudentId, e.CourseId, e.Date }).IsUnique();
            entity.HasIndex(e => new { e.CourseId, e.Date });

            entity.HasOne(e => e.Student)
                .WithMany(u => u.AttendanceAsStudent)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Course)
                .WithMany(c => c.AttendanceRecords)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RecordedBy)
                .WithMany(u => u.AttendanceAsTeacher)
                .HasForeignKey(e => e.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CourseCalendarEntry>(entity =>
        {
            entity.ToTable("CALENDARIOCURSO");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CourseId).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.IsLective).IsRequired();
            entity.Property(e => e.DayType).HasMaxLength(50);
            entity.Property(e => e.Module).HasMaxLength(500);
            entity.Property(e => e.Teacher).HasMaxLength(200);
            entity.Property(e => e.Room).HasMaxLength(50);
            entity.Property(e => e.UploadedAt).IsRequired();

            entity.HasIndex(e => new { e.CourseId, e.Date }).IsUnique();

            entity.HasOne(e => e.Course)
                .WithMany(c => c.CalendarEntries)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
