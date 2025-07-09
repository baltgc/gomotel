using System.ComponentModel.DataAnnotations;

namespace Gomotel.Infrastructure.MercadoPago.Models;

/// <summary>
/// Request model for creating a MercadoPago payment
/// </summary>
public class MercadoPagoPaymentRequest
{
    /// <summary>
    /// Payment amount
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., "USD", "ARS")
    /// </summary>
    [Required]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment description
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// External reference (our internal payment ID)
    /// </summary>
    [Required]
    public string ExternalReference { get; set; } = string.Empty;

    /// <summary>
    /// Payment method ID (e.g., "visa", "master", "pix")
    /// </summary>
    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Card token (for card payments)
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Number of installments
    /// </summary>
    public int Installments { get; set; } = 1;

    /// <summary>
    /// Payer information
    /// </summary>
    [Required]
    public MercadoPagoPayerRequest Payer { get; set; } = new();

    /// <summary>
    /// Metadata for additional information
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Payer information for MercadoPago payment
/// </summary>
public class MercadoPagoPayerRequest
{
    /// <summary>
    /// Payer email
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Payer first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Payer last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Payer identification
    /// </summary>
    public MercadoPagoIdentification? Identification { get; set; }
}

/// <summary>
/// Identification information for MercadoPago payer
/// </summary>
public class MercadoPagoIdentification
{
    /// <summary>
    /// Identification type (e.g., "CPF", "CNPJ", "DNI")
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Identification number
    /// </summary>
    public string Number { get; set; } = string.Empty;
}
