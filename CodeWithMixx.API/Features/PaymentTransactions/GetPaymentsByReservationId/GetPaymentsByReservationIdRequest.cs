using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.PaymentTransactions.GetPaymentsByReservationId;

public record GetPaymentsByReservationIdRequest(int ReservationId);