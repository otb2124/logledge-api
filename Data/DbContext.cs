using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using logledge_api.Models;

namespace logledge_api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Priority> Priorities => Set<Priority>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<BoardList> BoardLists => Set<BoardList>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketLabel> TicketLabels => Set<TicketLabel>();
    public DbSet<TicketLink> TicketLinks => Set<TicketLink>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>().Property(t => t.Type).HasConversion<string>();
        modelBuilder.Entity<TicketLink>().Property(l => l.Type).HasConversion<string>();

        modelBuilder.Entity<ProjectMember>()
            .HasKey(pm => new { pm.ProjectId, pm.UserId });

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Priority>()
            .HasOne(p => p.Project)
            .WithMany(proj => proj.Priorities)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Label>()
            .HasOne(l => l.Project)
            .WithMany(proj => proj.Labels)
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketLabel>()
            .HasKey(tl => new { tl.TicketId, tl.LabelId });

        modelBuilder.Entity<TicketLabel>()
            .HasOne(tl => tl.Ticket)
            .WithMany(t => t.Labels)
            .HasForeignKey(tl => tl.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketLabel>()
            .HasOne(tl => tl.Label)
            .WithMany(l => l.Tickets)
            .HasForeignKey(tl => tl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BoardList>()
            .HasOne(l => l.Project)
            .WithMany(p => p.Lists)
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ticket>()
    .HasOne(t => t.Project)
    .WithMany(p => p.Tickets)
    .HasForeignKey(t => t.ProjectId)
    .OnDelete(DeleteBehavior.Restrict); // CHANGED from Cascade to Restrict

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.List)
            .WithMany(l => l.Tickets)
            .HasForeignKey(t => t.ListId)
            .OnDelete(DeleteBehavior.Restrict); // Keeps list deletion clean

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Parent)
            .WithMany(t => t.Children)
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Reporter)
            .WithMany()
            .HasForeignKey(t => t.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Priority)
            .WithMany()
            .HasForeignKey(t => t.PriorityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TicketLink>()
            .HasOne(tl => tl.FromTicket)
            .WithMany(t => t.OutgoingLinks)
            .HasForeignKey(tl => tl.FromTicketId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketLink>()
            .HasOne(tl => tl.ToTicket)
            .WithMany(t => t.IncomingLinks)
            .HasForeignKey(tl => tl.ToTicketId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ReplyToComment)
            .WithMany()
            .HasForeignKey(c => c.ReplyToCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}