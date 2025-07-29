using Microsoft.EntityFrameworkCore;
using Sonic.Models;
using System.Text.Json;
namespace Sonic.API.Data;

public class SonicDbContext : DbContext
{
    public SonicDbContext(DbContextOptions<SonicDbContext> options)
        : base(options) { }
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Song> Songs { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Venue> Venues { get; set; } = null!;
    public DbSet<Instrument> Instruments { get; set; } = null!;
    public DbSet<PlaceAutocompleteResponse> PlaceAutocompleteResponses { get; set; } = null!;
    public DbSet<PlaceDetails> PlaceDetails { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PlaceAutocompleteResponse>()
            .Property(p => p.Suggestions)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Suggestion>>(v, (JsonSerializerOptions?)null) ?? new List<Suggestion>()
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<PlaceDetails>()
            .Property(p => p.AddressComponents)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<AddressComponent>>(v, (JsonSerializerOptions?)null) ?? new List<AddressComponent>()
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<PlaceDetails>()
            .Property(p => p.DisplayName)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<DisplayName>(v, (JsonSerializerOptions?)null) ?? null
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<PlaceDetails>()
            .Property(p => p.Location)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Location>(v, (JsonSerializerOptions?)null) ?? null
            )
            .HasColumnType("jsonb");

        // ✅ Configure Event entity explicitly
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);


            // Configure the Venue relationship explicitly
            entity.HasOne(e => e.Venue)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JSON property
            entity.Property(e => e.ExternalSources)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ExternalSource>>(v, (JsonSerializerOptions?)null) ?? new List<ExternalSource>()
                )
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasMany(v => v.Events)
                .WithOne(e => e.Venue)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure JSON property
            entity.Property(v => v.ExternalSources)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ExternalSource>>(v, (JsonSerializerOptions?)null) ?? new List<ExternalSource>()
                )
                .HasColumnType("jsonb");

            entity.Property(v => v.Address)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Address>(v, (JsonSerializerOptions?)null) ?? null
                )
                .HasColumnType("jsonb");
        });

        // ✅ Configure User-Event many-to-many relationship
        modelBuilder.Entity<User>()
            .HasMany(u => u.Events)
            .WithMany(e => e.Attendees);

        // Configure other entities...
        modelBuilder.Entity<Song>()
            .HasMany(s => s.RequiredInstruments)
            .WithMany()
            .UsingEntity(j => j.ToTable("SongRequiredInstruments"));

        modelBuilder.Entity<Song>()
            .HasMany(s => s.OptionalInstruments)
            .WithMany()
            .UsingEntity(j => j.ToTable("SongOptionalInstruments"));

        // Configure other JSON properties
        modelBuilder.Entity<Song>()
            .Property(s => s.ExternalSources)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ExternalSource>>(v, (JsonSerializerOptions?)null) ?? new List<ExternalSource>()
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<Venue>()
            .Property(v => v.ExternalSources)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ExternalSource>>(v, (JsonSerializerOptions?)null) ?? new List<ExternalSource>()
            )
            .HasColumnType("jsonb");

        // ✅ Fixed seed data with static values
        modelBuilder.Entity<Instrument>().HasData(
            new Instrument { Id = 1, Name = "Lead Guitar", Emoji = "🎸", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Uuid = new Guid("11111111-1111-1111-1111-111111111111") },
            new Instrument { Id = 2, Name = "Rhythm Guitar", Emoji = "🎸", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Uuid = new Guid("22222222-2222-2222-2222-222222222222") },
            new Instrument { Id = 3, Name = "Bass", Emoji = "🎸", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Uuid = new Guid("33333333-3333-3333-3333-333333333333") },
            new Instrument { Id = 4, Name = "Drums", Emoji = "🥁", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Uuid = new Guid("44444444-4444-4444-4444-444444444444") },
            new Instrument { Id = 5, Name = "Keyboard", Emoji = "🎹", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Uuid = new Guid("55555555-5555-5555-5555-555555555555") },
            new Instrument { Id = 6, Name = "Vocals", Emoji = "🎤", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Uuid = new Guid("66666666-6666-6666-6666-666666666666") },
            new Instrument { Id = 7, Name = "Backup Vocals", Emoji = "🎤", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), Uuid = new Guid("77777777-7777-7777-7777-777777777777") }
        );
    }
}
