using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Application.Common.Errors;

public static class ApplicationErrors
{
    public static Error WorkOrderOutsideOperatingHours
        => Error.Conflict(
            "The work order schedule is outside of operating hours.",
            "ApplicationErrors.WorkOrder.OutsideOperatingHours");

    public static Error WorkOrderNotFound
        => Error.NotFound("The work order was not found.", "ApplicationErrors.WorkOrder.NotFound");

    public static Error LaborOccupied
        => Error.Conflict("The labor is already occupied.", "ApplicationErrors.Labor.Occupied");

    public static Error CustomerNotFound
        => Error.NotFound("The customer was not found.", "ApplicationErrors.Customer.NotFound");

    public static Error VehicleNotFound
        => Error.NotFound("The vehicle was not found.", "ApplicationErrors.Vehicle.NotFound");

    public static Error VehicleSchedulingConflict
        => Error.Conflict("The vehicle has a scheduling conflict.", "ApplicationErrors.Vehicle.SchedulingConflict");

    public static Error RepairTaskNotFound
        => Error.NotFound("The repair task was not found.", "ApplicationErrors.RepairTask.NotFound");

    public static Error InvoiceNotFound
        => Error.NotFound("The invoice was not found.", "ApplicationErrors.Invoice.NotFound");

    public static Error InvoicePdfGenerationFailed
        => Error.Unexpected("Failed to generate the invoice PDF.", "ApplicationErrors.Invoice.PdfGenerationFailed");

    public static Error InvalidRefreshToken
        => Error.Validation("The refresh token is invalid.", "ApplicationErrors.Authentication.InvalidRefreshToken");

    public static Error ExpiredAccessTokenInvalid
        => Error.Validation(
            "The access token has expired and is invalid.",
            "ApplicationErrors.Authentication.ExpiredAccessTokenInvalid");

    public static Error UserIdClaimInvalid
        => Error.Validation("The user ID claim is invalid.", "ApplicationErrors.Authentication.UserIdClaimInvalid");

    public static Error RefreshTokenExpired
        => Error.Validation("The refresh token has expired.", "ApplicationErrors.Authentication.RefreshTokenExpired");

    public static Error UserNotFound
        => Error.NotFound("The user was not found.", "ApplicationErrors.Authentication.UserNotFound");

    public static Error TokenGenerationFailed
        => Error.Validation("Token generation failed.", "ApplicationErrors.Authentication.TokenGenerationFailed");

    public static Error AuthenticationFailed
        => Error.Unauthorized("Authentication failed.", "ApplicationErrors.Authentication.AuthenticationFailed");

    public static Error RefreshTokenReuseDetected
        => Error.Unauthorized(
            "Suspicious activity detected. All sessions have been revoked.",
            "ApplicationErrors.Authentication.RefreshTokenReuseDetected");

    public static Error RefreshTokenFingerprintMissing
        => Error.Unauthorized(
            "The refresh token is missing required device metadata.",
            "ApplicationErrors.Authentication.RefreshTokenFingerprintMissing");

    public static Error RefreshTokenFingerprintMismatch
        => Error.Unauthorized(
            "The refresh token device verification failed.",
            "ApplicationErrors.Authentication.RefreshTokenFingerprintMismatch");

    public static Error LaborNotFound
        => Error.NotFound("The labor was not found.", "ApplicationErrors.Labor.NotFound");

    public static Error ConcurrencyConflict
        => Error.Conflict(
            "The record was modified by another user. Please refresh and try again.",
            "ApplicationErrors.Database.ConcurrencyConflict");

    public static Error DatabaseError
        => Error.Unexpected(
            "A database error occurred while saving changes.",
            "ApplicationErrors.Database.Error");

    public static Error CustomerHasActiveWorkOrders
        => Error.Conflict(
            "Cannot delete a customer with active or scheduled work orders.",
            "ApplicationErrors.Customer.HasActiveWorkOrders");

    public static Error RepairTaskInUse
        => Error.Conflict(
            "The repair task is currently in use by a work order and cannot be removed.",
            "ApplicationErrors.RepairTask.InUse");

    public static Error WorkOrderSpotNotAvailable
        => Error.Conflict(
            "The work order spot is not available.",
            "ApplicationErrors.WorkOrder.SpotNotAvailable");

    public static Error ReadonlyWorkOrder
         => Error.Validation(
            "The work order cannot be modified because it is in a read-only state.",
            "ApplicationErrors.WorkOrder.ReadOnly");

    public static Error CustomerEmailAlreadyExists
        => Error.Conflict("A customer with the same email already exists.", "ApplicationErrors.Customer.EmailAlreadyExists");
}