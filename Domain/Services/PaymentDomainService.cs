using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.Events;
using Gomotel.Domain.Exceptions;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Domain.Services;

public class PaymentDomainService
{
    public Payment CreatePayment(Guid reservationId, Money amount, string paymentMethod)
    {
        ValidatePaymentCreation(reservationId, amount, paymentMethod);

        return new Payment(reservationId, amount, paymentMethod);
    }

    public void ProcessPayment(Payment payment)
    {
        ValidatePaymentProcessing(payment);

        payment.Status = PaymentStatus.Processing;
        payment.MarkAsUpdated();
    }

    public void ApprovePayment(Payment payment, string transactionId)
    {
        ValidatePaymentApproval(payment, transactionId);

        payment.Status = PaymentStatus.Approved;
        payment.TransactionId = transactionId;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.MarkAsUpdated();

        payment.AddEvent(new PaymentApprovedEvent(payment.Id, payment.ReservationId));
    }

    public void FailPayment(Payment payment, string reason)
    {
        ValidatePaymentFailure(payment, reason);

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = reason;
        payment.ProcessedAt = DateTime.UtcNow;
        payment.MarkAsUpdated();
    }

    public void RefundPayment(Payment payment)
    {
        ValidatePaymentRefund(payment);

        payment.Status = PaymentStatus.Refunded;
        payment.MarkAsUpdated();
    }

    public bool CanPaymentBeProcessed(Payment payment)
    {
        return payment.Status == PaymentStatus.Created;
    }

    public bool CanPaymentBeApproved(Payment payment)
    {
        return payment.Status == PaymentStatus.Processing;
    }

    public bool CanPaymentBeFailed(Payment payment)
    {
        return payment.Status is not (PaymentStatus.Approved or PaymentStatus.Refunded);
    }

    public bool CanPaymentBeRefunded(Payment payment)
    {
        return payment.Status == PaymentStatus.Approved;
    }

    public bool IsPaymentSuccessful(Payment payment)
    {
        return payment.Status == PaymentStatus.Approved;
    }

    public bool IsPaymentFailed(Payment payment)
    {
        return payment.Status == PaymentStatus.Failed;
    }

    public bool IsPaymentRefunded(Payment payment)
    {
        return payment.Status == PaymentStatus.Refunded;
    }

    private void ValidatePaymentCreation(Guid reservationId, Money amount, string paymentMethod)
    {
        if (reservationId == Guid.Empty)
            throw new ArgumentException("Reservation ID cannot be empty", nameof(reservationId));
        if (amount == null)
            throw new ArgumentNullException(nameof(amount));
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method cannot be empty", nameof(paymentMethod));
    }

    private void ValidatePaymentProcessing(Payment payment)
    {
        if (payment.Status != PaymentStatus.Created)
            throw new InvalidOperationException(
                "Only created payments can be marked as processing"
            );
    }

    private void ValidatePaymentApproval(Payment payment, string transactionId)
    {
        if (payment.Status != PaymentStatus.Processing)
            throw new InvalidOperationException("Only processing payments can be approved");
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));
    }

    private void ValidatePaymentFailure(Payment payment, string reason)
    {
        if (payment.Status is PaymentStatus.Approved or PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot fail approved or refunded payments");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be empty", nameof(reason));
    }

    private void ValidatePaymentRefund(Payment payment)
    {
        if (payment.Status != PaymentStatus.Approved)
            throw new InvalidOperationException("Only approved payments can be refunded");
    }
}
