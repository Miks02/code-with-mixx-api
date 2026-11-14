using CodeWithMixx.API.Domain.Entities.Inquiries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.HasKey(x => x.Id);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Inquiries_ResolvedAtAfterCreatedAt",
                $"\"{nameof(Inquiry.ResolvedAt)}\" IS NULL OR \"{nameof(Inquiry.ResolvedAt)}\" > \"{nameof(Inquiry.CreatedAt)}\"");
        });

        builder.HasOne(x => x.Admin)
            .WithMany(a => a.Inquiries)
            .HasForeignKey(x => x.ResolvedByAdmin)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .IsRequired();

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(InquiryConstants.MaxMessageLength);

        builder.Property(x => x.PhoneNumber)
            .IsRequired();
        
        builder.Property(x => x.InquiryType)
            .IsRequired()
            .HasConversion<string>();
        
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ResolvedAt);
        builder.HasIndex(x => x.InquiryType);
        
        
    }
}