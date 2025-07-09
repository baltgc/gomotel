using Gomotel.Infrastructure.MercadoPago.Models;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gomotel.Infrastructure.MercadoPago;

/// <summary>
/// MercadoPago payment service implementation
/// </summary>
public class MercadoPagoPaymentService : IMercadoPagoPaymentService
{
    private readonly MercadoPagoSettings _settings;
    private readonly ILogger<MercadoPagoPaymentService> _logger;
    private readonly PaymentClient _paymentClient;

    public MercadoPagoPaymentService(
        IOptions<MercadoPagoSettings> settings,
        ILogger<MercadoPagoPaymentService> logger
    )
    {
        _settings = settings.Value;
        _logger = logger;

        if (!_settings.IsValid)
        {
            throw new InvalidOperationException("MercadoPago settings are not properly configured");
        }

        // Configure MercadoPago SDK
        MercadoPagoConfig.AccessToken = _settings.AccessToken;
        _paymentClient = new PaymentClient();
    }

    /// <inheritdoc />
    public async Task<MercadoPagoPaymentResponse> CreatePaymentAsync(
        MercadoPagoPaymentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Creating MercadoPago payment for reference: {ExternalReference}",
                request.ExternalReference
            );

            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = request.Amount,
                Description = request.Description,
                PaymentMethodId = request.PaymentMethodId,
                Payer = new PaymentPayerRequest
                {
                    Email = request.Payer.Email,
                    FirstName = request.Payer.FirstName,
                    LastName = request.Payer.LastName,
                    Identification =
                        request.Payer.Identification != null
                            ? new IdentificationRequest
                            {
                                Type = request.Payer.Identification.Type,
                                Number = request.Payer.Identification.Number,
                            }
                            : null,
                },
                ExternalReference = request.ExternalReference,
                Metadata = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                Token = request.Token,
                Installments = request.Installments,
                NotificationUrl = _settings.WebhookUrl,
            };

            var payment = await _paymentClient.CreateAsync(
                paymentRequest,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "MercadoPago payment created successfully. ID: {PaymentId}, Status: {Status}",
                payment.Id,
                payment.Status
            );

            return MapToPaymentResponse(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating MercadoPago payment for reference: {ExternalReference}",
                request.ExternalReference
            );
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MercadoPagoPaymentResponse> GetPaymentAsync(
        long paymentId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation("Getting MercadoPago payment: {PaymentId}", paymentId);

            var payment = await _paymentClient.GetAsync(
                paymentId,
                cancellationToken: cancellationToken
            );

            return MapToPaymentResponse(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MercadoPago payment: {PaymentId}", paymentId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MercadoPagoPaymentResponse?> GetPaymentByExternalReferenceAsync(
        string externalReference,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Getting MercadoPago payment by external reference: {ExternalReference}",
                externalReference
            );

            // Note: This is a simplified implementation. In a real-world scenario,
            // you might need to implement pagination or use a different approach
            // to find payments by external reference.

            _logger.LogWarning(
                "GetPaymentByExternalReferenceAsync is not fully implemented in this version"
            );
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting MercadoPago payment by external reference: {ExternalReference}",
                externalReference
            );
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MercadoPagoPaymentResponse> CancelPaymentAsync(
        long paymentId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation("Cancelling MercadoPago payment: {PaymentId}", paymentId);

            var payment = await _paymentClient.CancelAsync(
                paymentId,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "MercadoPago payment cancelled successfully. ID: {PaymentId}",
                paymentId
            );

            return MapToPaymentResponse(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling MercadoPago payment: {PaymentId}", paymentId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MercadoPagoRefundResponse> RefundPaymentAsync(
        long paymentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Refunding MercadoPago payment: {PaymentId}, Amount: {Amount}",
                paymentId,
                amount
            );

            // Note: This is a simplified implementation. The actual refund implementation
            // would require proper setup of refund requests with the MercadoPago SDK.

            _logger.LogWarning("RefundPaymentAsync is not fully implemented in this version");

            return new MercadoPagoRefundResponse
            {
                Id = 0,
                PaymentId = paymentId,
                Amount = amount ?? 0,
                Status = "pending",
                DateCreated = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding MercadoPago payment: {PaymentId}", paymentId);
            throw;
        }
    }

    private static MercadoPagoPaymentResponse MapToPaymentResponse(Payment payment)
    {
        return new MercadoPagoPaymentResponse
        {
            Id = payment.Id ?? 0,
            Status = payment.Status ?? string.Empty,
            StatusDetail = payment.StatusDetail ?? string.Empty,
            ExternalReference = payment.ExternalReference ?? string.Empty,
            Amount = payment.TransactionAmount ?? 0,
            Currency = payment.CurrencyId ?? string.Empty,
            Description = payment.Description ?? string.Empty,
            PaymentMethodId = payment.PaymentMethodId ?? string.Empty,
            PaymentTypeId = payment.PaymentTypeId ?? string.Empty,
            DateCreated = payment.DateCreated ?? DateTime.UtcNow,
            DateLastUpdated = payment.DateLastUpdated ?? DateTime.UtcNow,
            DateApproved = payment.DateApproved,
            TransactionDetails = new MercadoPagoTransactionDetails
            {
                NetReceivedAmount = payment.TransactionDetails?.NetReceivedAmount ?? 0,
                TotalPaidAmount = payment.TransactionDetails?.TotalPaidAmount ?? 0,
                OverpaidAmount = payment.TransactionDetails?.OverpaidAmount ?? 0,
                Installments = 1, // Default value since SDK doesn't provide this
                FinancialInstitution = payment.TransactionDetails?.FinancialInstitution,
                PaymentMethodReferenceId = payment.TransactionDetails?.PaymentMethodReferenceId,
            },
            Payer = new MercadoPagoPayerResponse
            {
                Id = payment.Payer?.Id ?? string.Empty,
                Email = payment.Payer?.Email ?? string.Empty,
                FirstName = payment.Payer?.FirstName ?? string.Empty,
                LastName = payment.Payer?.LastName ?? string.Empty,
                Identification =
                    payment.Payer?.Identification != null
                        ? new MercadoPagoIdentification
                        {
                            Type = payment.Payer.Identification.Type ?? string.Empty,
                            Number = payment.Payer.Identification.Number ?? string.Empty,
                        }
                        : null,
            },
            Metadata =
                payment.Metadata?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.ToString() ?? string.Empty
                ) ?? new Dictionary<string, string>(),
            AuthorizationCode = payment.AuthorizationCode,
            Capture = true, // Default value since SDK doesn't provide this
        };
    }
}
