using Microsoft.EntityFrameworkCore;
using SupportTickets.Api.Domain.Entities;

namespace SupportTickets.Api.Infrastructure.Persistence;

public sealed class TicketDbContext(DbContextOptions<TicketDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var ticket = modelBuilder.Entity<Ticket>();
        ticket.ToTable("Tickets", table =>
        {
            table.HasCheckConstraint("CK_Tickets_Status", "[status] IN ('open', 'in_progress', 'closed')");
            table.HasCheckConstraint("CK_Tickets_Priority", "[priority] IN ('low', 'medium', 'high')");
        });
        ticket.HasKey(item => item.Id);
        ticket.Property(item => item.Id).HasColumnName("id");
        ticket.Property(item => item.Title).HasColumnName("title").HasMaxLength(120).IsRequired();
        ticket.Property(item => item.Description).HasColumnName("description").HasMaxLength(2000);
        ticket.Property(item => item.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        ticket.Property(item => item.Priority).HasColumnName("priority").HasMaxLength(10).IsRequired();
        ticket.Property(item => item.CreatedAt).HasColumnName("created_at");
        ticket.Property(item => item.UpdatedAt).HasColumnName("updated_at");
        ticket.HasIndex(item => item.Status);
        ticket.HasIndex(item => item.Priority);
        ticket.HasIndex(item => item.CreatedAt);

        var account = modelBuilder.Entity<UserAccount>();
        account.ToTable("UserAccounts");
        account.HasKey(item => item.Id);
        account.Property(item => item.Id).HasColumnName("id");
        account.Property(item => item.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        account.Property(item => item.NormalizedEmail)
            .HasColumnName("normalized_email")
            .HasMaxLength(254)
            .IsRequired();
        account.Property(item => item.PasswordHash).HasColumnName("password_hash").IsRequired();
        account.Property(item => item.CreatedAt).HasColumnName("created_at");
        account.HasIndex(item => item.NormalizedEmail).IsUnique();
    }
}
