using Microsoft.EntityFrameworkCore;
using SecureFileStorage.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<SecureFileStorage.Models.File> Files { get; set; }
    public DbSet<Folder> Folders { get; set; }
    public DbSet<FileVersion> FileVersions { get; set; }
    public DbSet<SharedFile> SharedFiles { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; } 
    public DbSet<Permission> Permissions { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; } // RefreshToken DbSet'i
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // İlişkileri ve kısıtlamaları burada yapılandırabilirsiniz.
        // Örneğin:
        modelBuilder.Entity<SecureFileStorage.Models.File>()
            .HasOne(f => f.Owner)
            .WithMany(u => u.Files)
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // Silme işlemlerini kısıtla

        modelBuilder.Entity<SecureFileStorage.Models.File>()
            .HasOne(f => f.Folder)
            .WithMany(fo => fo.Files)
            .HasForeignKey(f => f.FolderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FileVersion>()
            .HasOne(fv => fv.File)
            .WithMany(f => f.Versions)
            .HasForeignKey(fv => fv.FileId);

        modelBuilder.Entity<SharedFile>()
           .HasOne(sf => sf.File)
           .WithMany(f => f.SharedFiles)
           .HasForeignKey(sf => sf.FileId);
    }
}