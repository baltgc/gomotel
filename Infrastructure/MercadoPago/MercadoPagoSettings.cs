namespace Gomotel.Infrastructure.MercadoPago;

/// <summary>
/// MercadoPago configuration settings
/// </summary>
public class MercadoPagoSettings
{
    public const string SectionName = "MercadoPago";

    /// <summary>
    /// MercadoPago access token for API authentication
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// MercadoPago public key for frontend integration
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret key for webhook verification
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Environment: "Sandbox" or "Production"
    /// </summary>
    public string Environment { get; set; } = "Sandbox";

    /// <summary>
    /// Default currency for payments
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment timeout in minutes
    /// </summary>
    public int PaymentTimeoutMinutes { get; set; } = 30;

    /// <summary>
    /// Webhook URL for payment notifications
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Check if the configuration is valid
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(AccessToken)
        && !string.IsNullOrWhiteSpace(PublicKey)
        && !string.IsNullOrWhiteSpace(Environment);
}
