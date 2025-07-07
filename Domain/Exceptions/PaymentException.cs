using System;
using Gomotel.Domain.Enums;

namespace Gomotel.Domain.Exceptions;

/// <summary>
/// Exception thrown when payment processing fails
/// </summary>
public class PaymentProcessingException : DomainException
{
    public Guid PaymentId { get; }
    public string PaymentMethod { get; }

    public PaymentProcessingException(Guid paymentId, string paymentMethod, string reason)
        : base($"Payment {paymentId} using {paymentMethod} failed: {reason}")
    {
        PaymentId = paymentId;
        PaymentMethod = paymentMethod;
    }

    public PaymentProcessingException(
        Guid paymentId,
        string paymentMethod,
        string reason,
        Exception innerException
    )
        : base($"Payment {paymentId} using {paymentMethod} failed: {reason}", innerException)
    {
        PaymentId = paymentId;
        PaymentMethod = paymentMethod;
    }
}

/// <summary>
/// Exception thrown when payment amount is invalid
/// </summary>
public class InvalidPaymentAmountException : DomainException
{
    public decimal RequestedAmount { get; }
    public decimal ExpectedAmount { get; }
    public string Currency { get; }

    public InvalidPaymentAmountException(
        decimal requestedAmount,
        decimal expectedAmount,
        string currency
    )
        : base(
            $"Payment amount {requestedAmount:C} {currency} does not match expected amount {expectedAmount:C} {currency}"
        )
    {
        RequestedAmount = requestedAmount;
        ExpectedAmount = expectedAmount;
        Currency = currency;
    }
}

/// <summary>
/// Exception thrown when attempting to process payment in invalid state
/// </summary>
public class InvalidPaymentStateException : DomainException
{
    public Guid PaymentId { get; }
    public PaymentStatus CurrentStatus { get; }
    public PaymentStatus RequiredStatus { get; }

    public InvalidPaymentStateException(
        Guid paymentId,
        PaymentStatus currentStatus,
        PaymentStatus requiredStatus
    )
        : base(
            $"Payment {paymentId} is in '{currentStatus}' status, but '{requiredStatus}' status is required for this operation"
        )
    {
        PaymentId = paymentId;
        CurrentStatus = currentStatus;
        RequiredStatus = requiredStatus;
    }
}

/// <summary>
/// Exception thrown when payment has already been processed
/// </summary>
public class PaymentAlreadyProcessedException : DomainException
{
    public Guid PaymentId { get; }
    public PaymentStatus Status { get; }

    public PaymentAlreadyProcessedException(Guid paymentId, PaymentStatus status)
        : base($"Payment {paymentId} has already been processed with status '{status}'")
    {
        PaymentId = paymentId;
        Status = status;
    }
}

/// <summary>
/// Exception thrown when payment refund fails
/// </summary>
public class PaymentRefundException : DomainException
{
    public Guid PaymentId { get; }
    public decimal RefundAmount { get; }

    public PaymentRefundException(Guid paymentId, decimal refundAmount, string reason)
        : base($"Failed to refund {refundAmount:C} for payment {paymentId}: {reason}")
    {
        PaymentId = paymentId;
        RefundAmount = refundAmount;
    }
}
