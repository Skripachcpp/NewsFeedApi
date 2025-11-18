using Domain.Entities;

namespace Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class EfContext(DbContextOptions<EfContext> options): DbContext(options) {
  public DbSet<NewsArticle> NewsArticles { get; set; }
  public DbSet<Tag> Tags { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<NewsArticle>(entity => {
      entity.ToTable("news_article");
      entity.HasKey(e => e.Id);
      entity.HasIndex(c => c.Id).IsUnique();
      entity.HasIndex(e => e.PublicationDate);
      
      entity.Property(e => e.Id)
        .HasColumnName("id")
        .ValueGeneratedOnAdd();
      entity.Property(e => e.Title)
        .HasColumnName("title")
        .HasMaxLength(500);
      entity.Property(e => e.Content)
        .HasColumnName("content")
        .IsRequired();
      entity.Property(e => e.Summary)
        .HasColumnName("summary")
        .HasMaxLength(1000);
      entity.Property(e => e.PublicationDate)
        .HasColumnName("publication_date")
        .IsRequired();
      entity.Property(e => e.UserId)
        .HasColumnName("user_id");
      entity.HasIndex(e => e.UserId);
      entity.Property(e => e.UserName)
        .HasColumnName("user_name")
        .HasMaxLength(200);
    });
    
    modelBuilder.Entity<Tag>(entity => {
      entity.ToTable("tag");
      entity.HasKey(e => e.Id);
      entity.HasIndex(c => c.Id).IsUnique();
      entity.HasIndex(e => e.Name).IsUnique();
      
      entity.Property(e => e.Id)
        .HasColumnName("id")
        .ValueGeneratedOnAdd();
      entity.Property(e => e.Name)
        .HasColumnName("name")
        .IsRequired()
        .HasMaxLength(100);
    });
  }
}