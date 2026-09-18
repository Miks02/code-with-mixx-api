using CodeWithMixx.API.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations;

public class ProjectNoteConfiguration : IEntityTypeConfiguration<ProjectNote>
{
    public void Configure(EntityTypeBuilder<ProjectNote> builder)
    {
        builder.ToTable("ProjectNotes");
        builder.HasKey(pn => pn.Id);
        
        builder.Property(pn => pn.Content)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(pn => pn.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.HasOne<Project>()
            .WithMany(p => p.ProjectNotes)
            .HasForeignKey(pn => pn.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}