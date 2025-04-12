using Microsoft.EntityFrameworkCore;
using WebHookNotifierMaui.Models;

namespace WebHookNotifierMaui.Data
{
    /// <summary>
    /// Database context for notification history
    /// </summary>
    public class NotificationDbContext : DbContext
    {
        public DbSet<NotificationHistory> Notifications { get; set; }
        
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure the NotificationHistory entity
            modelBuilder.Entity<NotificationHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Event).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.AdditionalDataJson).HasColumnName("AdditionalData");
            });
        }
    }
}
