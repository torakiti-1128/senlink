using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenLink.Domain.Modules.Request.Entities;
using SenLink.Domain.Modules.Request.Enums;

namespace SenLink.Infrastructure.Modules.Request.Persistence.Configurations;

/// <summary>
/// 申請添付ファイルのテーブル構成定義
/// </summary>
public class RequestAttachmentConfiguration : IEntityTypeConfiguration<RequestAttachment>
{
    public void Configure(EntityTypeBuilder<RequestAttachment> builder)
    {
        builder.ToTable("request_attachments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // 申請ID (FK, NN)
        builder.Property(e => e.RequestId).IsRequired();

        // Storage上のパス (VARCHAR(255), NN)
        builder.Property(e => e.FilePath).IsRequired().HasMaxLength(255);

        // ファイル形式 (SMALLINT, NN, Default: 0)
        builder.Property(e => e.FileType).IsRequired().HasDefaultValue((FileType)0);

        // 補足 (VARCHAR(255))
        builder.Property(e => e.Description).HasMaxLength(255);

        // 複合ユニーク制約: UQ(request_id, file_path)
        builder.HasIndex(e => new { e.RequestId, e.FilePath }).IsUnique();

        // Request とのリレーション (多対1)
        builder.HasOne(e => e.Request)
            .WithMany(r => r.Attachments)
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