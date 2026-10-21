using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Domain.ValueObjects.Monies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeWithMixx.API.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(pt => pt.Id);
        
        builder.ToTable(t => 
            t.HasCheckConstraint($"CK_PaymentTransactions_{nameof(PaymentTransaction.Amount)}_Positive", $"\"{nameof(PaymentTransaction.Amount)}\" > 0"));

        builder.ComplexProperty(t => t.Amount, money =>
        {
            money.Property(m => m.Value)
                .HasColumnName(nameof(PaymentTransaction.Amount))
                .HasPrecision(18, 2)
                .IsRequired();
        });
        
        builder.HasOne(pt => pt.Reservation)
            .WithMany(r => r.PaymentTransactions)
            .HasForeignKey(pt => pt.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(pt => pt.PaymentType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(pt => pt.TransactionDate)
            .IsRequired();

    }
}