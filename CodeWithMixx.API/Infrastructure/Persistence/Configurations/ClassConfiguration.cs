using CodeWithMixx.API.Domain.Entities.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.ToTable(t =>
            {
                t.HasCheckConstraint($"CK_{nameof(Class)}_Price_Positive", "\"Price\" >= 0");
                t.HasCheckConstraint($"CK_{nameof(Class)}_EndsAtAfterStartsAt", "\"EndsAt\" > \"StartsAt\"");
                t.HasCheckConstraint($"CK_{nameof(Class)}_ReservedAtBeforeStartsAt", "\"ReservedAt\" <= (\"StartsAt\" AT TIME ZONE 'Europe/Belgrade')::date");
                t.HasCheckConstraint($"CK_{nameof(Class)}_MaxDuration45Min", "\"EndsAt\" - \"StartsAt\" <= interval '45 minutes'");
            });
            
            builder.Property(r => r.Price)
                .HasPrecision(18, 2);

            builder.HasIndex(c => c.StartsAt);
            builder.HasIndex(c => c.EndsAt);

            builder.HasOne(c => c.Subject)
                .WithMany(s => s.Classes)
                .HasForeignKey(c => c.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Reservation)
                .WithMany(r => r.Classes)
                .HasForeignKey(c => c.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Property(c => c.ReservedAt)
                .IsRequired();
        }
    }
}
