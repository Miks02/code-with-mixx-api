using CodeWithMixx.API.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable(t => t.HasCheckConstraint("CK_Reservations_TotalPrice_Positive", "\"TotalPrice\" >= 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_Reservations_Bonus_Positive", "\"Bonus\" >= 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_Reservations_DiscountRate_Positive", "\"DiscountRate\" >= 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_Reservations_DiscountRate_LessThan100", "\"DiscountRate\" <= 100"));

            builder.Property(r => r.TotalPrice)
                .HasPrecision(18, 2);

            builder.Property(r => r.PaidAmount)
                .HasPrecision(18, 2);

            builder.Property(r => r.Bonus)
                .HasPrecision(18, 2);

            builder.Property(r => r.DiscountRate)
                .HasPrecision(5, 2);

            builder.Property(r => r.PaymentStatus)
                .HasConversion<string>();
            
            builder.Property(r => r.ReservationStatus)
                .HasConversion<string>();

            builder.Property(r => r.Notes)
                .HasMaxLength(500);

            builder.HasIndex(r => r.PaymentStatus);
            builder.HasIndex(r => r.ReservationStatus);

            builder.HasOne(r => r.Admin)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Student)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
