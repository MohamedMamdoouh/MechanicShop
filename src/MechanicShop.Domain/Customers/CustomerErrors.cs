using MechanicShop.Domain.Common.Results;
namespace MechanicShop.Domain.Customers;

public static class CustomerErrors
{
    public static Error FirstNameRequired
        => Error.Validation("Customer first name is required.", "Customer.FirstName.Required");

    public static Error LastNameRequired
        => Error.Validation("Customer last name is required.", "Customer.LastName.Required");

    public static Error EmailRequired
        => Error.Validation("Customer email is required.", "Customer.Email.Required");

    public static Error EmailInvalid
        => Error.Validation("Customer email is invalid.", "Customer.Email.Invalid");

    public static Error EmailAlreadyExists
        => Error.Validation("A customer with the same email already exists.", "Customer.Email.AlreadyExists");

    public static Error PhoneNumberRequired
        => Error.Validation("Customer phone number is required.", "Customer.PhoneNumber.Required");

    public static Error NotFound
        => Error.NotFound("Customer not found.", "Customer.NotFound");
}