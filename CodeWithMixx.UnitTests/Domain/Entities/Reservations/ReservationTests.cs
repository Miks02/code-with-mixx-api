using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions.TransactionNotes;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Domain.ValueObjects.Monies;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Reservations;

public class ReservationTests
{
    private const string StudentId = "student-1";
    private const string AdminId = "admin-1";
    private const int SubjectId = 3;
    private const int ReservationId = 11;
    private const decimal ClassPrice = ClassConstants.DefaultPrice;
    private const decimal ProjectPrice = 25000m;
    private const string RefundNote = "Povraćaj zbog otkazanog časa.";

    private static readonly DateOnly Today = new(2026, 11, 15);
    private static readonly DateOnly ReservedAt = new(2026, 11, 10);
    private static readonly DateTime FutureStart = new(2030, 1, 10, 16, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PastStart = new(2026, 1, 10, 16, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly PastReservedAt = new(2026, 1, 5);
    private static readonly DateTime TransactionDate = DateTime.UtcNow.AddDays(-1);

    private readonly Reservation _sut;

    public ReservationTests()
    {
        _sut = CreateClassReservation().Payload!;
        SetId(_sut, ReservationId);
    }

    [Fact]
    public void CreateClassReservation_WithValidData_ShouldSucceedAndMapValues()
    {
        var result = CreateClassReservation(description: "Priprema za kolokvijum");

        result.IsSuccess.Should().BeTrue();
        var reservation = result.Payload!;
        reservation.StudentId.Should().Be(StudentId);
        reservation.AdminId.Should().Be(AdminId);
        reservation.ReservedAt.Should().Be(ReservedAt);
        reservation.ReservationStatus.Should().Be(ReservationStatus.Confirmed);
        reservation.ReservationType.Should().Be(ReservationType.Class);
        reservation.Description.Should().Be("Priprema za kolokvijum");
        reservation.Classes.Should().HaveCount(2);
        reservation.Projects.Should().BeEmpty();
    }

    [Fact]
    public void CreateClassReservation_WithoutTotalPrice_ShouldUseSumOfClassPricesAndStartPending()
    {
        _sut.TotalPrice.Should().Be(2 * ClassPrice);
        _sut.PaidAmount.Should().Be(0);
        _sut.RemainingAmount.Should().Be(2 * ClassPrice);
        _sut.DiscountRate.Should().Be(0);
        _sut.Bonus.Should().Be(0);
        _sut.PaymentStatus.Should().Be(PaymentStatus.Pending);
        _sut.PaymentTransactions.Should().BeEmpty();
    }

    [Fact]
    public void CreateClassReservation_ShouldStartNotDeletedWithUnsetAuditFields()
    {
        var reservation = CreateClassReservation().Payload!;

        reservation.Id.Should().Be(0);
        reservation.CreatedAt.Should().Be(default);
        reservation.UpdatedAt.Should().BeNull();
        reservation.IsDeleted.Should().BeFalse();
        reservation.DeletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.Completed)]
    [InlineData(ReservationStatus.Cancelled)]
    public void CreateClassReservation_WithEveryReservationStatus_ShouldMapStatus(ReservationStatus status)
        => CreateClassReservation(status: status).Payload!.ReservationStatus.Should().Be(status);

    [Fact]
    public void CreateClassReservation_WithLowerTotalPrice_ShouldCalculateDiscountRate()
    {
        var reservation = CreateClassReservation(totalPrice: 2000m).Payload!;

        reservation.TotalPrice.Should().Be(2000m);
        reservation.DiscountRate.Should().Be(23.08m);
    }

    [Fact]
    public void CreateClassReservation_WithHigherTotalPrice_ShouldHaveNoDiscount()
    {
        var reservation = CreateClassReservation(totalPrice: 5000m).Payload!;

        reservation.TotalPrice.Should().Be(5000m);
        reservation.DiscountRate.Should().Be(0);
    }

    [Fact]
    public void CreateClassReservation_WithZeroTotalPrice_ShouldHaveFullDiscountAndBePaid()
    {
        var reservation = CreateClassReservation(totalPrice: 0).Payload!;

        reservation.DiscountRate.Should().Be(100);
        reservation.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public void CreateClassReservation_WithNegativeTotalPrice_ShouldReturnInvalidTotalPrice()
        => AssertFails(CreateClassReservation(totalPrice: -1m), ReservationError.InvalidTotalPrice(-1m));

    [Fact]
    public void CreateClassReservation_WithoutClasses_ShouldReturnNoClassesProvided()
        => AssertFails(CreateClassReservation(classes: []), ReservationError.NoClassesProvided());

    [Fact]
    public void CreateClassReservation_WithoutClassesAndNegativeTotal_ShouldReportNoClassesFirst()
        => AssertFails(CreateClassReservation(classes: [], totalPrice: -1m), ReservationError.NoClassesProvided());

    [Fact]
    public void CreateClassReservation_WithDescriptionAtMaxLength_ShouldSucceed()
        => CreateClassReservation(description: new string('a', ReservationConstants.MaxDescriptionLength)).IsSuccess.Should().BeTrue();

    [Fact]
    public void CreateClassReservation_WithDescriptionOverMaxLength_ShouldReturnDescriptionTooLong()
        => AssertFails(CreateClassReservation(description: new string('a', ReservationConstants.MaxDescriptionLength + 1)), ReservationError.DescriptionTooLong());

    [Fact]
    public void CreateClassReservation_WithWhitespaceDescriptionOverMaxLength_ShouldReturnDescriptionTooLong()
        => AssertFails(CreateClassReservation(description: new string(' ', ReservationConstants.MaxDescriptionLength + 1)), ReservationError.DescriptionTooLong());

    [Fact]
    public void CreateClassReservation_WithInvalidClass_ShouldReturnClassError()
        => AssertFails(CreateClassReservation(classes: [ClassData(FutureStart, price: 0)]), ClassError.InvalidPrice(0));

    [Fact]
    public void CreateClassReservation_WithInvalidClassSubject_ShouldReturnSubjectNotFound()
        => AssertFails(CreateClassReservation(classes: [ClassData(FutureStart) with { SubjectId = 0 }]), SubjectError.NotFound(0));

    [Fact]
    public void CreateClassReservation_WithOverlappingClasses_ShouldReturnClassesOverlap()
        => AssertFails(CreateClassReservation(classes: [ClassData(FutureStart), ClassData(FutureStart.AddMinutes(30))]), ReservationError.ClassesOverlap());

    [Fact]
    public void CreateClassReservation_WithClassesInReverseOrder_ShouldReturnClassesOverlap()
        => AssertFails(CreateClassReservation(classes: [ClassData(FutureStart.AddMinutes(45)), ClassData(FutureStart)]), ReservationError.ClassesOverlap());

    [Fact]
    public void CreateClassReservation_WithBackToBackClasses_ShouldSucceed()
        => CreateClassReservation(classes: [ClassData(FutureStart), ClassData(FutureStart.AddMinutes(45))]).IsSuccess.Should().BeTrue();

    [Fact]
    public void CreateClassReservation_WithBreakAtMaxLength_ShouldSucceed()
    {
        var secondStart = FutureStart.AddMinutes(45 + ClassConstants.MaxBreakBetweenClassesInMinutes);

        CreateClassReservation(classes: [ClassData(FutureStart), ClassData(secondStart)]).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateClassReservation_WithBreakOverMaxLength_ShouldReturnBreakDurationExceeded()
    {
        var secondStart = FutureStart.AddMinutes(45 + ClassConstants.MaxBreakBetweenClassesInMinutes + 1);

        AssertFails(CreateClassReservation(classes: [ClassData(FutureStart), ClassData(secondStart)]), ReservationError.BreakDurationExceeded());
    }

    [Fact]
    public void CreateClassReservation_WithDefaultReservedAt_ShouldReturnInvalidReservedAt()
        => AssertFails(CreateClassReservation(reservedAt: DateOnly.MinValue), ReservationError.InvalidReservedAt(DateOnly.MinValue));

    [Fact]
    public void CreateClassReservation_WithReservedAtInTheFuture_ShouldReturnInvalidReservedAt()
    {
        var reservedAt = Today.AddDays(1);

        AssertFails(CreateClassReservation(reservedAt: reservedAt), ReservationError.InvalidReservedAt(reservedAt));
    }

    [Fact]
    public void CreateClassReservation_WithReservedAtToday_ShouldSucceed()
        => CreateClassReservation(reservedAt: Today).IsSuccess.Should().BeTrue();

    [Fact]
    public void CreateClassReservation_WithReservedAtAfterFirstClass_ShouldReturnInvalidReservedAt()
    {
        var classStart = new DateTime(2026, 11, 12, 16, 0, 0, DateTimeKind.Utc);
        var reservedAt = new DateOnly(2026, 11, 13);

        AssertFails(CreateClassReservation(classes: [ClassData(classStart)], reservedAt: reservedAt), ReservationError.InvalidReservedAt(reservedAt));
    }

    [Fact]
    public void CreateClassReservation_WithPastClassesAndNoPayment_ShouldBeOverdue()
        => CreatePastClassReservation().PaymentStatus.Should().Be(PaymentStatus.Overdue);

    [Fact]
    public void CreateProjectReservation_WithValidData_ShouldSucceedAndMapValues()
    {
        var result = CreateProjectReservation();

        result.IsSuccess.Should().BeTrue();
        var reservation = result.Payload!;
        reservation.ReservationType.Should().Be(ReservationType.Project);
        reservation.Projects.Should().ContainSingle();
        reservation.Classes.Should().BeEmpty();
        reservation.TotalPrice.Should().Be(ProjectPrice);
        reservation.DiscountRate.Should().Be(0);
        reservation.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void CreateProjectReservation_WithMultipleProjects_ShouldSumPrices()
    {
        var reservation = CreateProjectReservation(projects: [ProjectData(), ProjectData() with { Price = 15000m }]).Payload!;

        reservation.TotalPrice.Should().Be(ProjectPrice + 15000m);
    }

    [Fact]
    public void CreateProjectReservation_WithLowerTotalPrice_ShouldCalculateDiscountRate()
        => CreateProjectReservation(totalPrice: 20000m).Payload!.DiscountRate.Should().Be(20m);

    [Fact]
    public void CreateProjectReservation_WithoutProjects_ShouldReturnNoProjectsProvided()
        => AssertFails(CreateProjectReservation(projects: []), ReservationError.NoProjectsProvided());

    [Fact]
    public void CreateProjectReservation_WithNegativeTotalPrice_ShouldReturnInvalidTotalPrice()
        => AssertFails(CreateProjectReservation(totalPrice: -5m), ReservationError.InvalidTotalPrice(-5m));

    [Fact]
    public void CreateProjectReservation_WithDescriptionOverMaxLength_ShouldReturnDescriptionTooLong()
        => AssertFails(CreateProjectReservation(description: new string('a', ReservationConstants.MaxDescriptionLength + 1)), ReservationError.DescriptionTooLong());

    [Fact]
    public void CreateProjectReservation_WithProjectStartingBeforeReservation_ShouldReturnProjectStartsBeforeReservation()
    {
        var startDate = ReservedAt.AddDays(-1);
        var project = ProjectData() with { ReservedAt = startDate, StartDate = startDate };

        AssertFails(CreateProjectReservation(projects: [project]), ReservationError.ProjectStartsBeforeReservation(startDate, ReservedAt));
    }

    [Fact]
    public void CreateProjectReservation_WithInvalidProject_ShouldReturnProjectError()
        => AssertFails(CreateProjectReservation(projects: [ProjectData() with { Price = -1 }]), ProjectError.NegativePrice());

    [Fact]
    public void CreateProjectReservation_WithReservedAtInTheFuture_ShouldReturnInvalidReservedAt()
    {
        var reservedAt = Today.AddDays(1);

        AssertFails(CreateProjectReservation(reservedAt: reservedAt), ReservationError.InvalidReservedAt(reservedAt));
    }

    [Fact]
    public void CreateProjectReservation_WithDefaultReservedAt_ShouldReturnInvalidReservedAt()
        => AssertFails(CreateProjectReservation(reservedAt: DateOnly.MinValue), ReservationError.InvalidReservedAt(DateOnly.MinValue));

    [Fact]
    public void CreateProjectReservation_WithEndedProjectsAndNoPayment_ShouldBeOverdue()
    {
        var project = ProjectData() with { ReservedAt = PastReservedAt, StartDate = PastReservedAt, EndDate = PastReservedAt.AddDays(30) };

        var reservation = CreateProjectReservation(projects: [project], reservedAt: PastReservedAt).Payload!;

        reservation.PaymentStatus.Should().Be(PaymentStatus.Overdue);
    }

    [Fact]
    public void CreateProjectReservation_WithProjectEndingToday_ShouldNotBeOverdue()
    {
        var project = ProjectData() with { StartDate = ReservedAt, EndDate = Today };

        CreateProjectReservation(projects: [project]).Payload!.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Theory]
    [InlineData(ReservationStatus.Completed)]
    [InlineData(ReservationStatus.Cancelled)]
    public void UpdateReservationStatus_WhenActive_ShouldChangeStatus(ReservationStatus status)
    {
        var result = _sut.UpdateReservationStatus(status);

        result.IsSuccess.Should().BeTrue();
        _sut.ReservationStatus.Should().Be(status);
    }

    [Fact]
    public void UpdateReservationStatus_WhenDeleted_ShouldReturnCantUpdateAnArchivedReservation()
    {
        _sut.DeleteClassReservation();

        AssertFails(_sut.UpdateReservationStatus(ReservationStatus.Completed), ReservationError.CantUpdateAnArchivedReservation(ReservationId));
        _sut.ReservationStatus.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void UpdateReservedAt_WithValidDate_ShouldChangeReservedAt()
    {
        var result = _sut.UpdateReservedAt(Today, Today);

        result.IsSuccess.Should().BeTrue();
        _sut.ReservedAt.Should().Be(Today);
    }

    [Fact]
    public void UpdateReservedAt_WithFutureDate_ShouldReturnInvalidReservedAtAndKeepDate()
    {
        var reservedAt = Today.AddDays(1);

        AssertFails(_sut.UpdateReservedAt(reservedAt, Today), ReservationError.InvalidReservedAt(reservedAt));
        _sut.ReservedAt.Should().Be(ReservedAt);
    }

    [Fact]
    public void UpdateReservedAt_AfterFirstProjectStart_ShouldReturnInvalidReservedAt()
    {
        var reservation = CreateProjectReservation(projects: [ProjectData() with { StartDate = new DateOnly(2026, 11, 12) }]).Payload!;
        var reservedAt = new DateOnly(2026, 11, 13);

        AssertFails(reservation.UpdateReservedAt(reservedAt, Today), ReservationError.InvalidReservedAt(reservedAt));
    }

    [Fact]
    public void UpdateReservedAt_AfterAllProjectsDeleted_ShouldSucceed()
    {
        var reservation = CreateProjectReservation().Payload!;
        SetProjectIds(reservation);
        reservation.DeleteProjects([1]);

        var result = reservation.UpdateReservedAt(Today, Today);

        result.IsSuccess.Should().BeTrue();
        reservation.ReservedAt.Should().Be(Today);
    }

    [Fact]
    public void UpdateReservedAt_AfterAllClassesDeleted_ShouldSucceed()
    {
        SetClassIds(_sut);
        _sut.DeleteClasses([1, 2]);

        _sut.UpdateReservedAt(Today, Today).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void UpdateReservedAt_AfterAllProjectsDeleted_ShouldStillRejectFutureDate()
    {
        var reservation = CreateProjectReservation().Payload!;
        SetProjectIds(reservation);
        reservation.DeleteProjects([1]);
        var reservedAt = Today.AddDays(1);

        AssertFails(reservation.UpdateReservedAt(reservedAt, Today), ReservationError.InvalidReservedAt(reservedAt));
    }

    [Fact]
    public void UpdateReservedAt_WhenDeleted_ShouldReturnCantUpdateAnArchivedReservation()
    {
        _sut.DeleteClassReservation();

        AssertFails(_sut.UpdateReservedAt(Today, Today), ReservationError.CantUpdateAnArchivedReservation(ReservationId));
    }

    [Fact]
    public void UpdateTotalPrice_WithValue_ShouldSetTotalAndRecalculateDiscount()
    {
        var result = _sut.UpdateTotalPrice(1300m, Today);

        result.IsSuccess.Should().BeTrue();
        _sut.TotalPrice.Should().Be(1300m);
        _sut.DiscountRate.Should().Be(50m);
    }

    [Fact]
    public void UpdateTotalPrice_WithNull_ShouldResetToSumOfClassPrices()
    {
        var reservation = CreateClassReservation(totalPrice: 2000m).Payload!;

        reservation.UpdateTotalPrice(null, Today);

        reservation.TotalPrice.Should().Be(2 * ClassPrice);
        reservation.DiscountRate.Should().Be(0);
    }

    [Fact]
    public void UpdateTotalPrice_WithNegativeValue_ShouldReturnInvalidTotalPriceAndKeepTotal()
    {
        AssertFails(_sut.UpdateTotalPrice(-1m, Today), ReservationError.InvalidTotalPrice(-1m));
        _sut.TotalPrice.Should().Be(2 * ClassPrice);
    }

    [Fact]
    public void UpdateTotalPrice_BelowPaidAmount_ShouldBePaidWithBonus()
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);

        _sut.UpdateTotalPrice(800m, Today);

        _sut.PaymentStatus.Should().Be(PaymentStatus.Paid);
        _sut.Bonus.Should().Be(200m);
        _sut.RemainingAmount.Should().Be(0);
        _sut.DiscountRate.Should().Be(69.23m);
    }

    [Fact]
    public void UpdateTotalPrice_AbovePaidAmount_ShouldClearBonusAndBePartiallyPaid()
    {
        _sut.RegisterPayment(3000m, TransactionDate, Today);

        _sut.UpdateTotalPrice(4000m, Today);

        _sut.Bonus.Should().Be(0);
        _sut.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);
        _sut.RemainingAmount.Should().Be(1000m);
    }

    [Fact]
    public void RegisterPayment_WithPartialAmount_ShouldBePartiallyPaidAndAddTransaction()
    {
        var result = _sut.RegisterPayment(1000m, TransactionDate, Today, "Prva rata");

        result.IsSuccess.Should().BeTrue();
        _sut.PaidAmount.Should().Be(1000m);
        _sut.RemainingAmount.Should().Be(1600m);
        _sut.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);
        _sut.Bonus.Should().Be(0);
        var transaction = _sut.PaymentTransactions.Should().ContainSingle().Subject;
        transaction.Should().BeSameAs(result.Payload);
        transaction.Amount.Value.Should().Be(1000m);
        transaction.PaymentType.Should().Be(PaymentTransactionType.Payment);
        transaction.ReservationId.Should().Be(ReservationId);
        transaction.TransactionNote.Note.Should().Be("Prva rata");
    }

    [Fact]
    public void RegisterPayment_WithFullAmount_ShouldBePaid()
    {
        _sut.RegisterPayment(2 * ClassPrice, TransactionDate, Today);

        _sut.PaymentStatus.Should().Be(PaymentStatus.Paid);
        _sut.RemainingAmount.Should().Be(0);
        _sut.Bonus.Should().Be(0);
    }

    [Fact]
    public void RegisterPayment_InMultipleInstallments_ShouldAccumulate()
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);
        _sut.RegisterPayment(1600m, TransactionDate, Today);

        _sut.PaidAmount.Should().Be(2600m);
        _sut.PaymentStatus.Should().Be(PaymentStatus.Paid);
        _sut.PaymentTransactions.Should().HaveCount(2);
    }

    [Fact]
    public void RegisterPayment_OverTotalPrice_ShouldBePaidWithBonus()
    {
        _sut.RegisterPayment(3000m, TransactionDate, Today);

        _sut.PaymentStatus.Should().Be(PaymentStatus.Paid);
        _sut.Bonus.Should().Be(400m);
        _sut.RemainingAmount.Should().Be(0);
    }

    [Fact]
    public void RegisterPayment_WithMoreThanTwoDecimals_ShouldRoundAmount()
    {
        _sut.RegisterPayment(100.456m, TransactionDate, Today);

        _sut.PaidAmount.Should().Be(100.46m);
        _sut.PaymentTransactions.Single().Amount.Value.Should().Be(100.46m);
    }

    [Fact]
    public void RegisterPayment_PartialAfterClassesEnded_ShouldBeOverdue()
    {
        var reservation = CreatePastClassReservation();

        reservation.RegisterPayment(500m, TransactionDate, Today);

        reservation.PaymentStatus.Should().Be(PaymentStatus.Overdue);
    }

    [Fact]
    public void RegisterPayment_FullAfterClassesEnded_ShouldBePaid()
    {
        var reservation = CreatePastClassReservation();

        reservation.RegisterPayment(ClassPrice, TransactionDate, Today);

        reservation.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void RegisterPayment_WithNonPositiveAmount_ShouldReturnInvalidAmountAndNotChangeState(decimal amount)
    {
        AssertFails(_sut.RegisterPayment(amount, TransactionDate, Today), ReservationError.InvalidAmount(amount));
        AssertNoPayments(_sut);
    }

    [Fact]
    public void RegisterPayment_WithAmountRoundedToZero_ShouldReturnCannotBeZeroAndNotChangeState()
    {
        AssertFails(_sut.RegisterPayment(0.004m, TransactionDate, Today), PaymentTransactionError.CannotBeZero());
        AssertNoPayments(_sut);
    }

    [Fact]
    public void RegisterPayment_WithFutureTransactionDate_ShouldReturnErrorAndNotChangeState()
    {
        AssertFails(_sut.RegisterPayment(1000m, DateTime.UtcNow.AddDays(1), Today), PaymentTransactionError.PaymentDateCannotBeInTheFuture());
        AssertNoPayments(_sut);
    }

    [Fact]
    public void RegisterPayment_WithNoteOverMaxLength_ShouldReturnNoteTooLongAndNotChangeState()
    {
        AssertFails(_sut.RegisterPayment(1000m, TransactionDate, Today, new string('a', TransactionNoteConstants.MaxNoteLength + 1)), TransactionNoteError.NoteTooLong());
        AssertNoPayments(_sut);
    }

    [Fact]
    public void RegisterPayment_OnUnsavedReservation_ShouldReturnInvalidReservation()
    {
        var reservation = CreateClassReservation().Payload!;

        AssertFails(reservation.RegisterPayment(1000m, TransactionDate, Today), PaymentTransactionError.InvalidReservation(0));
    }

    [Theory]
    [InlineData(PaymentTransactionType.Refund)]
    [InlineData(PaymentTransactionType.Reversal)]
    public void SubtractPayment_WithValidAmount_ShouldReducePaidAmountAndAddTransaction(PaymentTransactionType type)
    {
        _sut.RegisterPayment(2000m, TransactionDate, Today);

        var result = _sut.SubtractPayment(500m, TransactionDate, type, Today, RefundNote);

        result.IsSuccess.Should().BeTrue();
        _sut.PaidAmount.Should().Be(1500m);
        _sut.PaymentStatus.Should().Be(PaymentStatus.PartiallyPaid);
        _sut.PaymentTransactions.Should().HaveCount(2);
        result.Payload!.PaymentType.Should().Be(type);
        result.Payload.Amount.Value.Should().Be(500m);
    }

    [Fact]
    public void SubtractPayment_WholePaidAmount_ShouldReturnToPending()
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);

        _sut.SubtractPayment(1000m, TransactionDate, PaymentTransactionType.Refund, Today, RefundNote);

        _sut.PaidAmount.Should().Be(0);
        _sut.PaymentStatus.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void SubtractPayment_FromOverpaidReservation_ShouldReduceBonus()
    {
        _sut.RegisterPayment(3000m, TransactionDate, Today);

        _sut.SubtractPayment(300m, TransactionDate, PaymentTransactionType.Refund, Today, RefundNote);

        _sut.Bonus.Should().Be(100m);
        _sut.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public void SubtractPayment_WithMoreThanTwoDecimals_ShouldSubtractRoundedAmount()
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);

        var result = _sut.SubtractPayment(100.456m, TransactionDate, PaymentTransactionType.Refund, Today, RefundNote);

        result.Payload!.Amount.Value.Should().Be(100.46m);
        _sut.PaidAmount.Should().Be(899.54m);
    }

    [Fact]
    public void SubtractPayment_WithPaymentType_ShouldReturnInvalidPaymentType()
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);

        AssertFails(_sut.SubtractPayment(100m, TransactionDate, PaymentTransactionType.Payment, Today, RefundNote), PaymentTransactionError.InvalidPaymentType(PaymentTransactionType.Payment));
        _sut.PaidAmount.Should().Be(1000m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void SubtractPayment_WithNonPositiveAmount_ShouldReturnInvalidAmount(decimal amount)
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);

        AssertFails(_sut.SubtractPayment(amount, TransactionDate, PaymentTransactionType.Refund, Today, RefundNote),
            ReservationError.InvalidAmount(amount, "Amount to subtract cannot be negative."));
        _sut.PaidAmount.Should().Be(1000m);
    }

    [Fact]
    public void SubtractPayment_MoreThanPaidAmount_ShouldReturnInvalidAmountAndNotChangeState()
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);

        AssertFails(_sut.SubtractPayment(1000.01m, TransactionDate, PaymentTransactionType.Refund, Today, RefundNote),
            ReservationError.InvalidAmount(1000.01m, "Cannot subtract more than the paid amount."));
        _sut.PaidAmount.Should().Be(1000m);
        _sut.PaymentTransactions.Should().ContainSingle();
    }

    [Fact]
    public void SubtractPayment_WithoutAnyPayment_ShouldReturnInvalidAmount()
        => AssertFails(_sut.SubtractPayment(100m, TransactionDate, PaymentTransactionType.Refund, Today, RefundNote),
            ReservationError.InvalidAmount(100m, "Cannot subtract more than the paid amount."));

    [Theory]
    [InlineData(PaymentTransactionType.Refund)]
    [InlineData(PaymentTransactionType.Reversal)]
    public void SubtractPayment_WithoutNote_ShouldReturnNoteErrorAndNotChangeState(PaymentTransactionType type)
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);

        var result = _sut.SubtractPayment(100m, TransactionDate, type, Today);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Single().Code.Should().StartWith("TransactionNote.Required");
        _sut.PaidAmount.Should().Be(1000m);
        _sut.PaymentTransactions.Should().ContainSingle();
    }

    [Fact]
    public void SubtractPayment_WithFutureTransactionDate_ShouldReturnErrorAndNotChangeState()
    {
        _sut.RegisterPayment(1000m, TransactionDate, Today);

        AssertFails(_sut.SubtractPayment(100m, DateTime.UtcNow.AddDays(1), PaymentTransactionType.Refund, Today, RefundNote), PaymentTransactionError.PaymentDateCannotBeInTheFuture());
        _sut.PaidAmount.Should().Be(1000m);
    }

    [Fact]
    public void SubtractPayment_PartialAfterClassesEnded_ShouldBeOverdue()
    {
        var reservation = CreatePastClassReservation();
        reservation.RegisterPayment(ClassPrice, TransactionDate, Today);

        reservation.SubtractPayment(100m, TransactionDate, PaymentTransactionType.Refund, Today, RefundNote);

        reservation.PaymentStatus.Should().Be(PaymentStatus.Overdue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Nova napomena")]
    public void UpdateDescription_WithValidValue_ShouldSetDescription(string? description)
    {
        var result = _sut.UpdateDescription(description);

        result.IsSuccess.Should().BeTrue();
        _sut.Description.Should().Be(description);
    }

    [Fact]
    public void UpdateDescription_OverMaxLengthOnSavedReservation_ShouldReturnDescriptionTooLongWithIdAndKeepDescription()
    {
        _sut.UpdateDescription("Stara napomena");

        AssertFails(_sut.UpdateDescription(new string('a', ReservationConstants.MaxDescriptionLength + 1)), ReservationError.DescriptionTooLong(ReservationId));
        _sut.Description.Should().Be("Stara napomena");
    }

    [Fact]
    public void ChangeStudent_ShouldSetStudentId()
    {
        _sut.ChangeStudent("student-2");

        _sut.StudentId.Should().Be("student-2");
    }

    [Fact]
    public void AddClasses_FollowingLastClass_ShouldAppendClass()
    {
        var result = _sut.AddClasses([ClassData(FutureStart.AddMinutes(90))]);

        result.IsSuccess.Should().BeTrue();
        _sut.Classes.Should().HaveCount(3);
    }

    [Fact]
    public void AddClasses_OverlappingLastClass_ShouldReturnClassesOverlap()
        => AssertFails(_sut.AddClasses([ClassData(FutureStart.AddMinutes(60))]), ReservationError.ClassesOverlap());

    [Fact]
    public void AddClasses_OnProjectReservation_ShouldReturnNotAClassReservation()
    {
        var reservation = CreateProjectReservation().Payload!;

        AssertFails(reservation.AddClasses([ClassData(FutureStart)]), ReservationError.NotAClassReservation(0));
        reservation.Classes.Should().BeEmpty();
    }

    [Fact]
    public void AddProjects_OnClassReservation_ShouldReturnNotAProjectReservation()
    {
        AssertFails(_sut.AddProjects([ProjectData()]), ReservationError.NotAProjectReservation(ReservationId));
        _sut.Projects.Should().BeEmpty();
    }

    [Fact]
    public void AddProjects_WithValidProject_ShouldAppendProject()
    {
        var reservation = CreateProjectReservation().Payload!;

        reservation.AddProjects([ProjectData() with { Price = 10000m }]).IsSuccess.Should().BeTrue();

        reservation.Projects.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateClasses_WithValidData_ShouldUpdateClassAndRecalculateDiscount()
    {
        var reservation = CreateClassReservation(totalPrice: 2000m).Payload!;
        SetClassIds(reservation);
        var first = reservation.Classes.First();

        var result = reservation.UpdateClasses([ClassUpdate(1, FutureStart, 1000m)], Today);

        result.IsSuccess.Should().BeTrue();
        first.Price.Should().Be(1000m);
        reservation.TotalPrice.Should().Be(2000m);
        reservation.DiscountRate.Should().Be(13.04m);
    }

    [Fact]
    public void UpdateClasses_WithUnknownClass_ShouldReturnClassNotFound()
    {
        SetClassIds(_sut);

        AssertFails(_sut.UpdateClasses([ClassUpdate(99, FutureStart, ClassPrice)], Today), ClassError.NotFound(99));
    }

    [Fact]
    public void UpdateClasses_WithInvalidClassData_ShouldReturnClassError()
    {
        SetClassIds(_sut);

        AssertFails(_sut.UpdateClasses([ClassUpdate(1, FutureStart, 0)], Today), ClassError.InvalidPrice(0));
    }

    [Fact]
    public void UpdateClasses_OnProjectReservation_ShouldReturnNotAClassReservation()
        => AssertFails(CreateProjectReservation().Payload!.UpdateClasses([], Today), ReservationError.NotAClassReservation(0));

    [Fact]
    public void UpdateProjects_WithValidData_ShouldUpdateProjectAndRecalculateDiscount()
    {
        var reservation = CreateProjectReservation(totalPrice: 20000m).Payload!;
        SetProjectIds(reservation);

        var result = reservation.UpdateProjects([ProjectUpdate(1) with { Price = 40000m }], Today);

        result.IsSuccess.Should().BeTrue();
        reservation.Projects.Single().Price.Should().Be(40000m);
        reservation.TotalPrice.Should().Be(20000m);
        reservation.DiscountRate.Should().Be(50m);
    }

    [Fact]
    public void UpdateProjects_WithUnknownProject_ShouldReturnProjectNotFound()
    {
        var reservation = CreateProjectReservation().Payload!;
        SetProjectIds(reservation);

        AssertFails(reservation.UpdateProjects([ProjectUpdate(99)], Today), ProjectError.NotFound(99));
    }

    [Fact]
    public void UpdateProjects_WithInvalidProjectData_ShouldReturnProjectError()
    {
        var reservation = CreateProjectReservation().Payload!;
        SetProjectIds(reservation);

        AssertFails(reservation.UpdateProjects([ProjectUpdate(1) with { Progress = 101 }], Today), ProjectError.InvalidProgress(101));
    }

    [Fact]
    public void UpdateProjects_OnClassReservation_ShouldReturnNotAProjectReservation()
        => AssertFails(_sut.UpdateProjects([], Today), ReservationError.NotAProjectReservation(ReservationId));

    [Fact]
    public void DeleteClasses_WithEmptyList_ShouldSucceedAndKeepClasses()
    {
        _sut.DeleteClasses([]).IsSuccess.Should().BeTrue();

        _sut.Classes.Should().HaveCount(2);
    }

    [Fact]
    public void DeleteClasses_WithExistingIds_ShouldRemoveThem()
    {
        SetClassIds(_sut);

        _sut.DeleteClasses([1]).IsSuccess.Should().BeTrue();

        _sut.Classes.Should().ContainSingle().Which.Id.Should().Be(2);
    }

    [Fact]
    public void DeleteClasses_WithUnknownId_ShouldReturnClassNotFoundAndKeepClasses()
    {
        SetClassIds(_sut);

        AssertFails(_sut.DeleteClasses([1, 99]), ClassError.NotFound(99));
        _sut.Classes.Should().HaveCount(2);
    }

    [Fact]
    public void DeleteProjects_WithExistingIds_ShouldRemoveThem()
    {
        var reservation = CreateProjectReservation(projects: [ProjectData(), ProjectData()]).Payload!;
        SetProjectIds(reservation);

        reservation.DeleteProjects([2]).IsSuccess.Should().BeTrue();

        reservation.Projects.Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Fact]
    public void DeleteProjects_WithEmptyList_ShouldSucceed()
        => CreateProjectReservation().Payload!.DeleteProjects([]).IsSuccess.Should().BeTrue();

    [Fact]
    public void DeleteProjects_WithUnknownId_ShouldReturnProjectNotFoundAndKeepProjects()
    {
        var reservation = CreateProjectReservation().Payload!;
        SetProjectIds(reservation);

        AssertFails(reservation.DeleteProjects([99]), ProjectError.NotFound(99));
        reservation.Projects.Should().ContainSingle();
    }

    [Fact]
    public void DeleteClassReservation_ShouldDeleteReservationAndAllClasses()
    {
        var before = DateTime.UtcNow;

        var result = _sut.DeleteClassReservation();

        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeTrue();
        _sut.DeletedAt.Should().NotBeNull();
        _sut.DeletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        _sut.DeletedAt.Value.Should().BeOnOrAfter(before);
        _sut.Classes.Should().OnlyContain(c => c.IsDeleted);
    }

    [Fact]
    public void DeleteClassReservation_OnProjectReservation_ShouldReturnNotAClassReservation()
    {
        var reservation = CreateProjectReservation().Payload!;

        AssertFails(reservation.DeleteClassReservation(), ReservationError.NotAClassReservation(0));
        reservation.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void DeleteClassReservation_WhenAlreadyDeletedWithoutLoadedClasses_ShouldReturnAlreadyDeleted()
    {
        _sut.DeleteClassReservation();
        SetClassIds(_sut);
        _sut.DeleteClasses([1, 2]);

        AssertFails(_sut.DeleteClassReservation(), ReservationError.AlreadyDeleted(ReservationId));
    }

    [Fact]
    public void DeleteProjectReservation_ShouldDeleteReservationAndAllProjects()
    {
        var reservation = CreateProjectReservation(projects: [ProjectData(), ProjectData()]).Payload!;

        reservation.DeleteProjectReservation().IsSuccess.Should().BeTrue();

        reservation.IsDeleted.Should().BeTrue();
        reservation.Projects.Should().OnlyContain(p => p.IsDeleted);
    }

    [Fact]
    public void DeleteProjectReservation_OnClassReservation_ShouldReturnNotAProjectReservation()
    {
        AssertFails(_sut.DeleteProjectReservation(), ReservationError.NotAProjectReservation(ReservationId));
        _sut.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void RestoreClassReservation_WhenDeleted_ShouldRestoreReservationAndClasses()
    {
        _sut.DeleteClassReservation();

        var result = _sut.RestoreClassReservation();

        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
        _sut.Classes.Should().OnlyContain(c => !c.IsDeleted);
    }

    [Fact]
    public void RestoreClassReservation_WhenNotDeleted_ShouldReturnNotArchived()
        => AssertFails(_sut.RestoreClassReservation(), ReservationError.NotArchived(ReservationId));

    [Fact]
    public void RestoreClassReservation_OnProjectReservation_ShouldReturnNotAClassReservation()
        => AssertFails(CreateProjectReservation().Payload!.RestoreClassReservation(), ReservationError.NotAClassReservation(0));

    [Fact]
    public void RestoreProjectReservation_WhenDeleted_ShouldRestoreReservationAndProjects()
    {
        var reservation = CreateProjectReservation().Payload!;
        reservation.DeleteProjectReservation();

        reservation.RestoreProjectReservation().IsSuccess.Should().BeTrue();

        reservation.IsDeleted.Should().BeFalse();
        reservation.Projects.Should().OnlyContain(p => !p.IsDeleted);
    }

    [Fact]
    public void RestoreProjectReservation_WhenNotDeleted_ShouldReturnNotArchived()
        => AssertFails(CreateProjectReservation().Payload!.RestoreProjectReservation(), ReservationError.NotArchived(0));

    [Fact]
    public void RestoreProjectReservation_OnClassReservation_ShouldReturnNotAProjectReservation()
        => AssertFails(_sut.RestoreProjectReservation(), ReservationError.NotAProjectReservation(ReservationId));

    [Fact]
    public void RequiresHistoryRetention_ForUntouchedFutureReservation_ShouldBeFalse()
        => _sut.RequiresHistoryRetention(Today).Should().BeFalse();

    [Fact]
    public void RequiresHistoryRetention_WhenCompleted_ShouldBeTrue()
    {
        _sut.UpdateReservationStatus(ReservationStatus.Completed);

        _sut.RequiresHistoryRetention(Today).Should().BeTrue();
    }

    [Fact]
    public void RequiresHistoryRetention_WhenCancelled_ShouldBeFalse()
    {
        _sut.UpdateReservationStatus(ReservationStatus.Cancelled);

        _sut.RequiresHistoryRetention(Today).Should().BeFalse();
    }

    [Fact]
    public void RequiresHistoryRetention_WhenPaymentRegistered_ShouldBeTrue()
    {
        _sut.RegisterPayment(100m, TransactionDate, Today);

        _sut.RequiresHistoryRetention(Today).Should().BeTrue();
    }

    [Fact]
    public void RequiresHistoryRetention_WhenClassAlreadyStarted_ShouldBeTrue()
        => CreatePastClassReservation().RequiresHistoryRetention(Today).Should().BeTrue();

    [Fact]
    public void RequiresHistoryRetention_ForFutureProjectReservation_ShouldBeFalse()
        => CreateProjectReservation().Payload!.RequiresHistoryRetention(Today).Should().BeFalse();

    [Fact]
    public void RequiresHistoryRetention_WhenProjectStartsToday_ShouldBeTrue()
    {
        var reservation = CreateProjectReservation(projects: [ProjectData() with { StartDate = Today }]).Payload!;

        reservation.PaymentStatus.Should().Be(PaymentStatus.Pending);
        reservation.RequiresHistoryRetention(Today).Should().BeTrue();
    }

    [Fact]
    public void Collections_ShouldNotBeMutableLists()
    {
        _sut.Classes.Should().NotBeAssignableTo<List<Class>>();
        _sut.Projects.Should().NotBeAssignableTo<List<Project>>();
        _sut.PaymentTransactions.Should().NotBeAssignableTo<List<PaymentTransaction>>();
    }

    private static API.Common.Results.Result<Reservation> CreateClassReservation(
        IReadOnlyList<ClassCreateData>? classes = null,
        decimal? totalPrice = null,
        DateOnly? reservedAt = null,
        string? description = null,
        ReservationStatus status = ReservationStatus.Confirmed)
        => Reservation.CreateClassReservation(new ReservationCreateData
        {
            StudentId = StudentId,
            AdminId = AdminId,
            ReservationStatus = status,
            TotalPrice = totalPrice,
            ReservedAt = reservedAt ?? ReservedAt,
            Description = description,
            Classes = classes ?? [ClassData(FutureStart), ClassData(FutureStart.AddMinutes(45))]
        }, Today);

    private static API.Common.Results.Result<Reservation> CreateProjectReservation(
        IReadOnlyList<ProjectCreateData>? projects = null,
        decimal? totalPrice = null,
        DateOnly? reservedAt = null,
        string? description = null)
        => Reservation.CreateProjectReservation(new ReservationCreateData
        {
            StudentId = StudentId,
            AdminId = AdminId,
            TotalPrice = totalPrice,
            ReservedAt = reservedAt ?? ReservedAt,
            Description = description
        }, projects ?? [ProjectData()], Today);

    private static Reservation CreatePastClassReservation()
    {
        var reservation = CreateClassReservation(classes: [ClassData(PastStart, reservedAt: PastReservedAt)], reservedAt: PastReservedAt).Payload!;
        SetId(reservation, ReservationId);
        return reservation;
    }

    private static ClassCreateData ClassData(DateTime startsAt, decimal price = ClassPrice, DateOnly? reservedAt = null) => new()
    {
        SubjectId = SubjectId,
        Price = price,
        StartsAt = startsAt,
        EndsAt = startsAt.AddMinutes(ClassConstants.MaxDurationInMinutes),
        ReservedAt = reservedAt ?? ReservedAt
    };

    private static ClassUpdateData ClassUpdate(int id, DateTime startsAt, decimal price) => new()
    {
        Id = id,
        SubjectId = SubjectId,
        Price = price,
        StartsAt = startsAt,
        EndsAt = startsAt.AddMinutes(ClassConstants.MaxDurationInMinutes),
        ReservedAt = ReservedAt
    };

    private static ProjectCreateData ProjectData() => new()
    {
        SubjectId = SubjectId,
        ProjectType = ProjectType.Seminar,
        ReservedAt = ReservedAt,
        StartDate = new DateOnly(2027, 1, 10),
        EndDate = new DateOnly(2027, 2, 10),
        Price = ProjectPrice,
        Today = Today
    };

    private static ProjectUpdateData ProjectUpdate(int id) => new()
    {
        Id = id,
        SubjectId = SubjectId,
        ProjectType = ProjectType.Seminar,
        ReservedAt = ReservedAt,
        StartDate = new DateOnly(2027, 1, 10),
        EndDate = new DateOnly(2027, 2, 10),
        Price = ProjectPrice,
        Today = Today
    };

    private static void SetId(object entity, int id)
        => entity.GetType().GetProperty("Id")!.SetValue(entity, id);

    private static void SetClassIds(Reservation reservation)
    {
        var id = 1;
        foreach (var cls in reservation.Classes)
            SetId(cls, id++);
    }

    private static void SetProjectIds(Reservation reservation)
    {
        var id = 1;
        foreach (var project in reservation.Projects)
            SetId(project, id++);
    }

    private static void AssertNoPayments(Reservation reservation)
    {
        reservation.PaidAmount.Should().Be(0);
        reservation.PaymentStatus.Should().Be(PaymentStatus.Pending);
        reservation.PaymentTransactions.Should().BeEmpty();
    }

    private static void AssertFails(API.Common.Results.Result result, Error expected)
    {
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(expected);
    }
}
