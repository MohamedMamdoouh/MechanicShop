namespace MechanicShop.Domain.Common.Results;

public readonly record struct Error
{
    private Error(ErrorKind kind, string description, string code)
    {
        Kind = kind;
        Description = description;
        Code = code;
    }

    public ErrorKind Kind { get; }
    public string Description { get; }
    public string Code { get; }

    public static Error Unexpected(string description = "An unexpected error occurred.", string code = nameof(Unexpected))
        => new(ErrorKind.Unexpected, description, code);

    public static Error NotFound(string description = "The requested resource was not found.", string code = nameof(NotFound))
        => new(ErrorKind.NotFound, description, code);

    public static Error Validation(string description = "The request data is invalid.", string code = nameof(Validation))
        => new(ErrorKind.Validation, description, code);

    public static Error Conflict(string description = "A conflict occurred.", string code = nameof(Conflict))
        => new(ErrorKind.Conflict, description, code);

    public static Error Unauthorized(string description = "Unauthorized access.", string code = nameof(Unauthorized))
        => new(ErrorKind.Unauthorized, description, code);

    public static Error Forbidden(string description = "Forbidden access.", string code = nameof(Forbidden))
        => new(ErrorKind.Forbidden, description, code);
}