using McDContactManager.Model;
using Microsoft.EntityFrameworkCore;

namespace McDContactManager.data;

public class DatabaseContext : DbContext
{
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=contacts.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>().HasKey(c => c.Id);
        
        modelBuilder.Entity<Contact>().Property(c => c.Id).ValueGeneratedOnAdd();
        
        modelBuilder.Entity<Contact>().Property(c => c.Name).IsRequired();
        modelBuilder.Entity<Contact>().Property(c => c.Email).IsRequired();
        modelBuilder.Entity<Contact>().Property(c => c.Phone).IsRequired();
        
        modelBuilder.Entity<Contact>().HasIndex(c => c.Phone).IsUnique();
    }
}