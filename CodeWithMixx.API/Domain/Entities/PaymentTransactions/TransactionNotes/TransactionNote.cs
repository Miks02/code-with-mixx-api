using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Domain.Entities.PaymentTransactions.TransactionNotes;

public record TransactionNote
{
    public string? Note { get; private init; } 
    
    public static TransactionNote Empty => new TransactionNote();

    public static Result<TransactionNote> Create(string? note, PaymentTransactionType paymentType)
    {
        if(paymentType == PaymentTransactionType.Refund && string.IsNullOrWhiteSpace(note))
            return Result<TransactionNote>.Failure(TransactionNoteError.NoteIsRequiredForRefund());
        
        if(paymentType == PaymentTransactionType.Reversal && string.IsNullOrWhiteSpace(note))
            return Result<TransactionNote>.Failure(TransactionNoteError.NoteIsRequiredForReversal());
        
        return Result<TransactionNote>.Success(new TransactionNote { Note = note });
    }
};