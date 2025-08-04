using Microsoft.EntityFrameworkCore;
using Sonic.Models;
using Sonic.API.Models.Tours;
using Sonic.API.Models.Budgets;
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
    public DbSet<Tour> Tours { get; set; } = null!;
    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<Instrument> Instruments { get; set; } = null!;
    public DbSet<PlaceAutocompleteResponse> PlaceAutocompleteResponses { get; set; } = null!;
    public DbSet<PlaceDetails> PlaceDetails { get; set; } = null!;
    public DbSet<Artist> Artists { get; set; } = null!;
    public DbSet<ResourceMembership> ResourceMemberships { get; set; } = null!;

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

        // configure user resoure membership
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.ResourceMemberships)
                .WithOne(rm => rm.User)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure external sources for artist
        modelBuilder.Entity<Artist>()
            .Property(a => a.ExternalSources)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ExternalSource>>(v, (JsonSerializerOptions?)null) ?? new List<ExternalSource>()
            )
            .HasColumnType("jsonb");

        // configure artist entity
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasMany(a => a.Events)
                .WithMany(e => e.Lineup)
                .UsingEntity(j => j.ToTable("ArtistEvents"));

            entity.HasMany(a => a.Members)
                .WithMany(u => u.Artists);
        });

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

            // Configure the many-to-many relationship with Users
            entity.HasMany(e => e.Attendees)
                .WithMany(u => u.AttendedEvents)
                .UsingEntity(j => j.ToTable("EventAttendees"));

            // Configure the many-to-many relationship with Organizers
            entity.HasMany(e => e.Organizers)
                .WithMany(u => u.OrganizedEvents)
                .UsingEntity(j => j.ToTable("EventOrganizers"));
        });

        // ✅ Configure Tour entity
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(t => t.Id);

            // Configure the many-to-many relationship with Artists (Users)
            entity.HasMany(t => t.Artists)
                .WithMany(u => u.Tours)
                .UsingEntity(j => j.ToTable("TourArtists"));

            // Configure the one-to-many relationship with Events (Shows)
            entity.HasMany(t => t.Shows)
                .WithOne(e => e.Tour)
                .HasForeignKey(e => e.TourId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ✅ Configure Budget entity
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(b => b.Id);

            // Configure optional relationship with Tour
            entity.HasOne(b => b.Tour)
                .WithMany()
                .HasForeignKey(b => b.TourId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure optional relationship with Event
            entity.HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure cascading ownership - Artist
            entity.HasOne(b => b.Artist)
                .WithMany()
                .HasForeignKey(b => b.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascading ownership - Venue
            entity.HasOne(b => b.Venue)
                .WithMany()
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship with Expenses
            entity.HasMany(b => b.Expenses)
                .WithOne(e => e.Budget)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure table with check constraint
            entity.ToTable("Budgets", t => 
                t.HasCheckConstraint("CK_Budget_Owner", 
                    "(\"ArtistId\" IS NOT NULL AND \"VenueId\" IS NULL) OR (\"ArtistId\" IS NULL AND \"VenueId\" IS NOT NULL)"));

            // Add indexes for performance
            entity.HasIndex(b => b.ArtistId);
            entity.HasIndex(b => b.VenueId);
            entity.HasIndex(b => b.TourId);
            entity.HasIndex(b => b.EventId);
            entity.HasIndex(b => new { b.StartDate, b.EndDate });
        });

        // ✅ Configure Expense entity
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configure required relationship with Budget
            entity.HasOne(e => e.Budget)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with SubmittedByUser
            entity.HasOne(e => e.SubmittedByUser)
                .WithMany()
                .HasForeignKey(e => e.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure optional relationship with ApprovedByUser
            entity.HasOne(e => e.ApprovedByUser)
                .WithMany()
                .HasForeignKey(e => e.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure JSON property for Attachments
            entity.Property(e => e.Attachments)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("jsonb");

            // Add indexes for performance
            entity.HasIndex(e => e.BudgetId);
            entity.HasIndex(e => e.SubmittedByUserId);
            entity.HasIndex(e => e.ApprovedByUserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpenseDate);
            entity.HasIndex(e => e.Category);
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

        // Confiugre contact infos as json in entities
        modelBuilder.Entity<Artist>()
            .Property(a => a.Contacts)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ContactInfo>>(v, (JsonSerializerOptions?)null) ?? new List<ContactInfo>()
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<Event>()
            .Property(e => e.Contacts)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ContactInfo>>(v, (JsonSerializerOptions?)null) ?? new List<ContactInfo>()
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<Venue>()
            .Property(v => v.Contacts)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ContactInfo>>(v, (JsonSerializerOptions?)null) ?? new List<ContactInfo>()
            )
            .HasColumnType("jsonb");

        modelBuilder.Entity<User>()
            .Property(u => u.Contacts)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ContactInfo>>(v, (JsonSerializerOptions?)null) ?? new List<ContactInfo>()
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
