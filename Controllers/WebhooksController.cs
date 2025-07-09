using Gomotel.Domain.Services;
using Gomotel.Infrastructure.MercadoPago;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gomotel.Controllers;

/// <summary>
/// Controller for handling webhook notifications from payment providers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Webhooks come from external services
public class WebhooksController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IMercadoPagoPaymentService _mercadoPagoService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IPaymentService paymentService,
        IMercadoPagoPaymentService mercadoPagoService,
        ILogger<WebhooksController> logger
    )
    {
        _paymentService = paymentService;
        _mercadoPagoService = mercadoPagoService;
        _logger = logger;
    }

    /// <summary>
    /// Handle MercadoPago webhook notifications
    /// </summary>
    /// <param name="request">Webhook notification data</param>
    /// <returns>Webhook response</returns>
    [HttpPost("mercadopago")]
    public async Task<IActionResult> MercadoPagoWebhook(
        [FromBody] MercadoPagoWebhookRequest request
    )
    {
        try
        {
            _logger.LogInformation(
                "Received MercadoPago webhook: {Type}, {Id}",
                request.Type,
                request.Id
            );

            if (request.Type == "payment")
            {
                await HandlePaymentWebhook(request.Id);
            }

            return Ok(new { status = "success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MercadoPago webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check endpoint for webhooks
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    private async Task HandlePaymentWebhook(long paymentId)
    {
        try
        {
            // Get payment details from MercadoPago
            var mercadoPagoPayment = await _mercadoPagoService.GetPaymentAsync(paymentId);

            if (string.IsNullOrEmpty(mercadoPagoPayment.ExternalReference))
            {
                _logger.LogWarning(
                    "MercadoPago payment {PaymentId} has no external reference",
                    paymentId
                );
                return;
            }

            if (!Guid.TryParse(mercadoPagoPayment.ExternalReference, out var internalPaymentId))
            {
                _logger.LogWarning(
                    "Invalid external reference format: {ExternalReference}",
                    mercadoPagoPayment.ExternalReference
                );
                return;
            }

            // Get our internal payment
            var payment = await _paymentService.GetPaymentByIdAsync(internalPaymentId);
            if (payment == null)
            {
                _logger.LogWarning(
                    "Payment not found for external reference: {ExternalReference}",
                    mercadoPagoPayment.ExternalReference
                );
                return;
            }

            // Update payment status based on MercadoPago status
            switch (mercadoPagoPayment.Status)
            {
                case "approved":
                    if (payment.Status != Domain.Enums.PaymentStatus.Approved)
                    {
                        await _paymentService.ApprovePaymentAsync(
                            internalPaymentId,
                            paymentId.ToString()
                        );
                        _logger.LogInformation(
                            "Payment {PaymentId} approved via webhook",
                            internalPaymentId
                        );
                    }
                    break;

                case "rejected":
                case "cancelled":
                    if (payment.Status != Domain.Enums.PaymentStatus.Failed)
                    {
                        await _paymentService.FailPaymentAsync(
                            internalPaymentId,
                            mercadoPagoPayment.StatusDetail
                        );
                        _logger.LogInformation(
                            "Payment {PaymentId} failed via webhook: {Reason}",
                            internalPaymentId,
                            mercadoPagoPayment.StatusDetail
                        );
                    }
                    break;

                case "refunded":
                    if (payment.Status != Domain.Enums.PaymentStatus.Refunded)
                    {
                        await _paymentService.RefundPaymentAsync(internalPaymentId);
                        _logger.LogInformation(
                            "Payment {PaymentId} refunded via webhook",
                            internalPaymentId
                        );
                    }
                    break;

                default:
                    _logger.LogInformation(
                        "No action required for payment {PaymentId} with status {Status}",
                        internalPaymentId,
                        mercadoPagoPayment.Status
                    );
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling payment webhook for MercadoPago payment {PaymentId}",
                paymentId
            );
            throw;
        }
    }
}

/// <summary>
/// MercadoPago webhook request model
/// </summary>
public class MercadoPagoWebhookRequest
{
    /// <summary>
    /// Type of notification (e.g., "payment")
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// ID of the resource (e.g., payment ID)
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Date when the event occurred
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// API version
    /// </summary>
    public string ApiVersion { get; set; } = string.Empty;

    /// <summary>
    /// Action performed (e.g., "payment.created", "payment.updated")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Whether the notification is a test
    /// </summary>
    public bool LiveMode { get; set; }
}
