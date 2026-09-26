using Papertrail.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Papertrail.Api.Persistence;

public sealed class PapertrailDbContext(DbContextOptions<PapertrailDbContext> options) : DbContext(options)
{
    public DbSet<GenerationHistoryEntry> GenerationHistory => Set<GenerationHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenerationHistoryEntry>(entity =>
        {
            entity.HasKey(entry => entry.RequestId);
            entity.Property(entry => entry.RequestId).HasMaxLength(32);
            entity.Property(entry => entry.ConversationId).HasMaxLength(100);
            entity.Property(entry => entry.Prompt).HasMaxLength(4000).IsRequired();
            entity.Property(entry => entry.Answer).IsRequired();
            entity.HasIndex(entry => new { entry.ConversationId, entry.CreatedAt });
        });
    }
}