using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Request.Entities;

namespace SenLink.Infrastructure.Modules.Request.Persistence.Configurations;

/// <summary>
/// 申請コメントのテーブル構成定義
/// </summary>
public class RequestCommentConfiguration : IEntityTypeConfiguration<RequestComment>
{
    public void Configure(EntityTypeBuilder<RequestComment> builder)
    {
        builder.ToTable("request_comments");
        builder.HasKey(e => e.Id);

        // 申請ID (FK, NN)
        builder.Property(e => e.RequestId).IsRequired();

        // 投稿者ID (NOFK, NN, accounts.id)
        builder.Property(e => e.AuthorAccountId).IsRequired();

        // コメント種別 (SMALLINT, NN)
        builder.Property(e => e.CommentType).IsRequired();

        // コメント本文 (TEXT, NN)
        builder.Property(e => e.Body).IsRequired().HasColumnType("text");

        // Request とのリレーション (多対1)
        builder.HasOne(e => e.Request)
            .WithMany(r => r.Comments)
            .HasForeignKey(e => e.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // 作成日時 (TIMESTAMP)
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
        
        // 更新日時 (TIMESTAMP)
        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}