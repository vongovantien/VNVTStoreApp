using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FW.WAPI.Core.Domain.IntegrationEventLog
{
    public class IntegrationEventLogContext : DbContext
    {
        public string IntegrationEventLogName { get; set; }
        public string InterationEventPKName { get; set; }

        public IntegrationEventLogContext(DbContextOptions<IntegrationEventLogContext> options,
            string interationEventPKName, string integrationEventLogName) : base(options)
        {
            IntegrationEventLogName = integrationEventLogName;
            InterationEventPKName = interationEventPKName;
        }

        public DbSet<IntegrationEventLogEntry> IntegrationEventLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntegrationEventLogEntry>(ConfigureIntegrationEventLogEntry);
        }

        void ConfigureIntegrationEventLogEntry(EntityTypeBuilder<IntegrationEventLogEntry> builder)
        {
            builder.ToTable(IntegrationEventLogName);

            builder.HasKey(e => e.EventId).HasName(InterationEventPKName);

            builder.Property(e => e.Content)
                .IsRequired();

            builder.Property(e => e.EventId)
                 .HasMaxLength(50)
                 .ValueGeneratedNever();
            builder.Property(e => e.CreationTime).HasColumnType("timestamp(6) without time zone");

            builder.Property(e => e.EventTypeName).HasMaxLength(500);

            builder.Property(e => e.State).HasMaxLength(255);

        }
    }
}
