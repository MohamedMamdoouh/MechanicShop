namespace MechanicShop.Domain.Common.Results;

public enum ErrorKind
{
    Unexpected,
    NotFound,
    Validation,
    Conflict,
    Unauthorized,
    Forbidden
}
