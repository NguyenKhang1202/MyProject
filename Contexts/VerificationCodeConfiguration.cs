using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Domain;

namespace MyProject.Context;

public class VerificationCodeConfiguration : IEntityTypeConfiguration<VerificationCode>
{
    public void Configure(EntityTypeBuilder<VerificationCode> builder)
    {
        builder.ToTable("VerificationCodes");

        builder.HasKey(vc => vc.CodeId);

        builder.Property(vc => vc.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(vc => vc.ExpiresAt)
            .IsRequired();

        builder.Property(vc => vc.CreatedAt)
            .HasDefaultValueSql("GETDATE()"); // Mặc định thời gian tạo là hiện tại

        builder.Property(vc => vc.IsUsed)
            .HasDefaultValue(false); // Mặc định là chưa sử dụng

        // Thiết lập quan hệ với bảng Users
        builder.HasOne(vc => vc.User)
            .WithMany(u => u.VerificationCodes)
            .HasForeignKey(vc => vc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
