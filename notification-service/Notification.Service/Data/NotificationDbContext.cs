using Microsoft.EntityFrameworkCore;
using Notification.Service.Models;

namespace Notification.Service.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options) { }

        public DbSet<ProductEvent> ProductEvents => Set<ProductEvent>();
    }
}
