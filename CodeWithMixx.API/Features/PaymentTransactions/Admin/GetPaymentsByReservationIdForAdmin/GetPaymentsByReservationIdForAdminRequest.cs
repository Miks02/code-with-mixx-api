using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.PaymentTransactions.Admin.GetPaymentsByReservationIdForAdmin;

public record GetPaymentsByReservationIdForAdminRequest(int ReservationId);