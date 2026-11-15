using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.PaymentTransactions.TransactionNotes;

public static class TransactionNoteError
{
    public static Error NoteIsRequiredForReversal()
        => new("TransactionNote.RequiredForReversal", "Note is required for reversed transaction.", ErrorType.Validation);
    
    public static Error NoteIsRequiredForRefund()
        => new("TransactionNote.RequiredForRefund", "Note is required for refunded transaction.", ErrorType.Validation);

    public static Error NoteTooLong()
        => new("TransactionNote.TooLong", $"Note cannot exceed {TransactionNoteConstants.MaxNoteLength} characters.", ErrorType.Validation);
}