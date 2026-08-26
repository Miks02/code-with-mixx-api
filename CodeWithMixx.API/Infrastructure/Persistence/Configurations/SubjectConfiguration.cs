using CodeWithMixx.API.Domain.Entities.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.HasIndex(x => x.Name)
            .IsUnique();
        
    }
}