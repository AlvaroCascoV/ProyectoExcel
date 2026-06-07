# SKILL: infrastructure-service
Use this skill when creating or modifying a service in `Attendance.Infrastructure/Services/`.

## When to trigger
- "Add a new service"
- "Add a method to an existing service"
- "Create the interface and implementation for X"

## Exact pattern

```csharp
// Attendance.Infrastructure/Services/ExampleService.cs
using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure.Services;

public interface IExampleService
{
    Task<ExampleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExampleDto>> GetByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> CreateAsync(CreateExampleRequest request, int actorId, CancellationToken cancellationToken = default);
}

public class ExampleService(ApplicationDbContext dbContext) : IExampleService
{
    public async Task<ExampleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Examples          // replace with real DbSet
            .AsNoTracking()                      // always AsNoTracking on reads
            .Where(e => e.Id == id)
            .Select(e => new ExampleDto(e.Id, e.Name))   // project in the query
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExampleDto>> GetByCourseAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Examples
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Name)
            .Select(e => new ExampleDto(e.Id, e.Name))
            .ToListAsync(cancellationToken);     // async always
    }

    public async Task<(bool Success, string? Error)> CreateAsync(
        CreateExampleRequest request,
        int actorId,
        CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Examples
            .AnyAsync(e => e.Id == request.Id, cancellationToken);

        if (!exists)
            return (false, $"Item {request.Id} not found.");

        var entity = new ExampleEntity { /* map from request */ };
        dbContext.Examples.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (true, null);
    }
}
```

## Rules
- Interface + implementation always together in the same file
- `AsNoTracking()` on every read — never omit
- Project into DTO inside the query (`.Select(e => new Dto(...))`) — do not load entities and map after
- Return `IReadOnlyList<T>` from public methods, never `List<T>`
- Return `(bool Success, string? Error)` tuple for write operations — the controller decides the HTTP status
- Never throw business exceptions — use result tuples
- Register in `ServiceCollectionExtensions.cs` with `.AddScoped<IExampleService, ExampleService>()`
- DTOs are `record` types in `Attendance.Infrastructure/DTOs/`

## DTO pattern
```csharp
// Attendance.Infrastructure/DTOs/ExampleDtos.cs
namespace Attendance.Infrastructure.DTOs;

public record ExampleDto(int Id, string Name);
public record CreateExampleRequest(int Id, string Name);
```

## Registration
```csharp
// Attendance.Infrastructure/Extensions/ServiceCollectionExtensions.cs
// Add inside AddAttendanceInfrastructure():
services.AddScoped<IExampleService, ExampleService>();
```

## EF entity pattern for NEW tables (not legacy)
```csharp
// Attendance.Infrastructure/Entities/ExampleEntity.cs
namespace Attendance.Infrastructure.Entities;

public class ExampleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // FK navigation properties
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
```

```csharp
// In ApplicationDbContext.OnModelCreating — Fluent API only, no data annotations
modelBuilder.Entity<ExampleEntity>(entity =>
{
    entity.ToTable("ExampleEntities");   // new EF-managed table, no ExcludeFromMigrations
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
    entity.HasOne(e => e.Course).WithMany().HasForeignKey(e => e.CourseId);
});
```
