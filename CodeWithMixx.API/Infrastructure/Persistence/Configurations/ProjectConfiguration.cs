using CodeWithMixx.API.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Projects_Price_NonNegative", "\"Price\" >= 0");
            t.HasCheckConstraint("CK_Projects_Progress_Range", "\"Progress\" >= 0 AND \"Progress\" <= 100");
            t.HasCheckConstraint("CK_Projects_Dates", "\"StartDate\" > '2000-01-01' AND \"StartDate\" <= \"EndDate\"");
        });
        
        builder.HasMany(p => p.ProjectNotes)
            .WithOne()
            .HasForeignKey(pn => pn.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Navigation(p => p.ProjectNotes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasOne(p => p.Reservation)
            .WithMany(r => r.Projects)
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.Subject)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.ProjectType)
            .IsRequired()
            .HasDefaultValue(ProjectType.Seminar)
            .HasConversion<string>();
        
        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(p => p.GithubLink)
            .HasMaxLength(200);
        
        builder.Property(p => p.DownloadLink)
            .HasMaxLength(200);
        
        builder.Property(p => p.Progress)
            .IsRequired()
            .HasDefaultValue(0m)
            .HasPrecision(5, 2);
        
        builder.Property(p => p.StartDate)
            .IsRequired();
        
        builder.Property(p => p.EndDate)
            .IsRequired();
        
    }
}