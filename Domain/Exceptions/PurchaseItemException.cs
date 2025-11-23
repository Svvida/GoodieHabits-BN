namespace Domain.Exceptions
{
    public class PurchaseItemException(string message) : AppException(message, 400)
    {
    }
}
