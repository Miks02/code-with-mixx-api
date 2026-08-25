using CodeWithMixx.API.Domain.Entities.RefreshTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(64);
        
        builder.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(64);
        
        builder.Property(x => x.CreatedByIp)
            .IsRequired()
            .HasMaxLength(45);
        
        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.TokenHash });

    }
}