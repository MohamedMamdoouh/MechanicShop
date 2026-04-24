namespace MechanicShop.Domain.Customers;

public interface IPhoneValidator
{
    bool IsValid(string phoneNumber);
    string? Normalize(string phoneNumber);
}
