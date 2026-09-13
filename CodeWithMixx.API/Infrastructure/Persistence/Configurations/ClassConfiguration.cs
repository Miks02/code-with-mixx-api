using CodeWithMixx.API.Domain.Entities.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.ToTable(t => t.HasCheckConstraint("CK_Classes_Price_Positive", "\"Price\" >= 0"));

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
        }
    }
}
