using System.IO;
using McDContactManager.Model;
using McDContactManager.Service;
using Microsoft.EntityFrameworkCore;

namespace McDContactManager.data;

public class DatabaseContext(string dbPath) : DbContext
{
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //var dbPath = Path.Combine(AppInitializer.AppFolderPath, "contacts.db");
        
        optionsBuilder.UseSqlite($"Data Source={Path.Combine(AppInitializer.AppFolderPath, dbPath)}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>().HasKey(c => c.Id);
        modelBuilder.Entity<Contact>().Property(c => c.Id).ValueGeneratedOnAdd();
        
        modelBuilder.Entity<Contact>().Property(c => c.Name).IsRequired();
        modelBuilder.Entity<Contact>().Property(c => c.Email).IsRequired();
        modelBuilder.Entity<Contact>().Property(c => c.Phone).IsRequired();
        modelBuilder.Entity<Contact>().Property(c => c.AssignedDate).IsRequired();

        modelBuilder.Entity<Contact>()
            .HasIndex(c => new { c.Name, c.Email, c.Phone })
            .IsUnique();
    }
}