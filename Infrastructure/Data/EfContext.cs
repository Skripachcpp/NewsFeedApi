using Domain.Entities;

namespace Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

public class EfContext(DbContextOptions<EfContext> options): DbContext(options)
{
    public DbSet<NewsArticle>? NewsArticles { get; init; }
    public DbSet<Tag>? Tags { get; init; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null) throw new ArgumentNullException(nameof(modelBuilder));

        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.ToTable("news_article");
            entity.HasKey(e => e.Id);
            entity.HasIndex(c => c.Id).IsUnique();
            entity.HasIndex(e => e.PublicationDate);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityColumn();
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

        modelBuilder.Entity<NewsArticle>()
            .HasMany<Tag>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "news_article_tag",
                join => join.HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("tag_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join => join.HasOne<NewsArticle>()
                    .WithMany()
                    .HasForeignKey("news_article_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("news_article_id", "tag_id");
                    join.ToTable("news_article_tag");
                });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tag");
            entity.HasKey(e => e.Id);
            entity.HasIndex(c => c.Id).IsUnique();
            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityColumn();
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(100);
        });
    }
}
