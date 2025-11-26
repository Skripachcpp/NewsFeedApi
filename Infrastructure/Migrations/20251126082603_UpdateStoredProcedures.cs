using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_articles_paged(INTEGER, INTEGER);");
            
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION get_articles_paged(
    p_offset INTEGER DEFAULT 0,
    p_count INTEGER DEFAULT 100,
    news_article_id INTEGER DEFAULT NULL
)
    RETURNS TABLE (
                      id INTEGER,
                      title CHARACTER VARYING(500),
                      content TEXT,
                      summary CHARACTER VARYING(1000),
                      publication_date TIMESTAMP WITH TIME ZONE,
                      user_name CHARACTER VARYING(200),
                      tags TEXT[]
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT
            na.id,
            na.title,
            na.content,
            na.summary,
            na.publication_date,
            na.user_name,
            COALESCE(
                    (SELECT array_agg(t.name ORDER BY t.name)::TEXT[]
                     FROM news_article_tag nat
                              INNER JOIN tag t ON nat.tag_id = t.id
                     WHERE nat.news_article_id = na.id),
                    ARRAY[]::text[]
            ) as tags
        FROM (
                 SELECT
                     news_article.id,
                     news_article.title,
                     news_article.content,
                     news_article.summary,
                     news_article.publication_date,
                     news_article.user_name
                 FROM news_article
                 WHERE news_article_id IS NULL OR news_article.id = news_article_id
                 ORDER BY news_article.publication_date DESC
                 LIMIT p_count OFFSET p_offset
             ) na
        ORDER BY na.publication_date DESC;
END;
$$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION get_articles_paged(
    p_offset INTEGER DEFAULT 0,
    p_count INTEGER DEFAULT 100
)
RETURNS TABLE (
  id INTEGER,
  title CHARACTER VARYING(500),
  content TEXT,
  summary CHARACTER VARYING(1000),
  publication_date TIMESTAMP WITH TIME ZONE,
  user_name CHARACTER VARYING(200),
  tags TEXT[]
) AS $$
BEGIN
    RETURN QUERY
        SELECT
            na.id,
            na.title,
            na.content,
            na.summary,
            na.publication_date,
            na.user_name,
            COALESCE(
                    (SELECT array_agg(t.name ORDER BY t.name)::TEXT[]
                     FROM news_article_tag nat
                              INNER JOIN tag t ON nat.tag_id = t.id
                     WHERE nat.news_article_id = na.id),
                    ARRAY[]::text[]
            ) as tags
        FROM (
                 SELECT
                     news_article.id,
                     news_article.title,
                     news_article.content,
                     news_article.summary,
                     news_article.publication_date,
                     news_article.user_name
                 FROM news_article
                 ORDER BY news_article.publication_date DESC
                 LIMIT p_count OFFSET p_offset
             ) na
        ORDER BY na.publication_date DESC;
END;
$$ LANGUAGE plpgsql;
            ");
        }
    }
}
