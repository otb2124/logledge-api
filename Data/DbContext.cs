using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using logledge_api.Models;

namespace logledge_api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardList> BoardLists => Set<BoardList>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>()
            .Property(t => t.Priority)
            .HasConversion<string>();

        modelBuilder.Entity<Board>()
            .HasOne(b => b.Owner)
            .WithMany()
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // don't cascade-delete a user's boards from Identity ops

        modelBuilder.Entity<BoardList>()
            .HasOne(l => l.Board)
            .WithMany(b => b.Lists)
            .HasForeignKey(l => l.BoardId)
            .OnDelete(DeleteBehavior.Cascade); // deleting a board deletes its lists

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.BoardList)
            .WithMany(l => l.Tickets)
            .HasForeignKey(t => t.BoardListId)
            .OnDelete(DeleteBehavior.Cascade); // deleting a list deletes its tickets

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull); // unassign, don't delete the ticket, if user is removed
    }
}