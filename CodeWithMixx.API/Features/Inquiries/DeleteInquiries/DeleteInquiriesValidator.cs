using CodeWithMixx.API.Domain.Entities.Inquiries;
using FluentValidation;

namespace CodeWithMixx.API.Features.Inquiries.DeleteInquiries;

public class DeleteInquiriesValidator : AbstractValidator<DeleteInquiriesRequest>
{
    public DeleteInquiriesValidator()
    {
        RuleFor(x => x.InquiryIds)
            .NotEmpty().WithMessage("At least one inquiry identifier is required.")
            .Must(ids => ids.Length <= InquiryConstants.MaxInquiryIds)
            .WithMessage($"Cannot delete more than {InquiryConstants.MaxInquiryIds} inquiries at once.");

        RuleForEach(x => x.InquiryIds)
            .GreaterThan(0).WithMessage("Inquiry identifier must be positive.");
    }
}
