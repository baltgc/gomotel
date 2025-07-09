namespace Gomotel.Infrastructure.MercadoPago.Models;

/// <summary>
/// Response model from MercadoPago payment creation
/// </summary>
public class MercadoPagoPaymentResponse
{
    /// <summary>
    /// MercadoPago payment ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Payment status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment status detail
    /// </summary>
    public string StatusDetail { get; set; } = string.Empty;

    /// <summary>
    /// External reference (our internal payment ID)
    /// </summary>
    public string ExternalReference { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Payment description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Payment method used
    /// </summary>
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Payment type (e.g., "credit_card", "debit_card", "bank_transfer")
    /// </summary>
    public string PaymentTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Date when payment was created
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Date when payment was last updated
    /// </summary>
    public DateTime DateLastUpdated { get; set; }

    /// <summary>
    /// Date when payment was approved (if applicable)
    /// </summary>
    public DateTime? DateApproved { get; set; }

    /// <summary>
    /// Transaction amount details
    /// </summary>
    public MercadoPagoTransactionDetails TransactionDetails { get; set; } = new();

    /// <summary>
    /// Payer information
    /// </summary>
    public MercadoPagoPayerResponse Payer { get; set; } = new();

    /// <summary>
    /// Payment metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Authorization code (for approved payments)
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Capture flag
    /// </summary>
    public bool Capture { get; set; }

    /// <summary>
    /// Whether payment is successful
    /// </summary>
    public bool IsSuccessful => Status == "approved";

    /// <summary>
    /// Whether payment failed
    /// </summary>
    public bool IsFailed => Status == "rejected";

    /// <summary>
    /// Whether payment is pending
    /// </summary>
    public bool IsPending => Status == "pending";
}

/// <summary>
/// Transaction details from MercadoPago
/// </summary>
public class MercadoPagoTransactionDetails
{
    /// <summary>
    /// Net amount received
    /// </summary>
    public decimal NetReceivedAmount { get; set; }

    /// <summary>
    /// Total paid amount
    /// </summary>
    public decimal TotalPaidAmount { get; set; }

    /// <summary>
    /// Overpaid amount
    /// </summary>
    public decimal OverpaidAmount { get; set; }

    /// <summary>
    /// Number of installments
    /// </summary>
    public int Installments { get; set; }

    /// <summary>
    /// Financial institution
    /// </summary>
    public string? FinancialInstitution { get; set; }

    /// <summary>
    /// Payment method reference ID
    /// </summary>
    public string? PaymentMethodReferenceId { get; set; }
}

/// <summary>
/// Payer information from MercadoPago response
/// </summary>
public class MercadoPagoPayerResponse
{
    /// <summary>
    /// Payer ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Payer email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Payer first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Payer last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Payer identification
    /// </summary>
    public MercadoPagoIdentification? Identification { get; set; }
}
